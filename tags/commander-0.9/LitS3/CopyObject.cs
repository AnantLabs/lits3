using System;

namespace LitS3
{
    // TODO: Support the additional headers like metadata copy/replace

    /// <summary>
    /// Copies an existing object in S3. Even if you successfully get a CopyObjectResponse
    /// without an exception, you should inspect it to see if any errors occurred while copying.
    /// </summary>
    public class CopyObjectRequest : S3Request<CopyObjectResponse>
    {
        public CopyObjectRequest(S3Service service, string bucketName, string sourceObjectKey,
            string destObjectKey)
            : this(service, bucketName, sourceObjectKey, bucketName, destObjectKey)
        {
        }

        public CopyObjectRequest(S3Service service, string sourceBucketName, string sourceObjectKey,
            string destBucketName, string destObjectKey)
            : base(service, "PUT", destBucketName, destObjectKey, null)
        {
            WebRequest.Headers[S3Headers.CopySource] = sourceBucketName + "/" + sourceObjectKey;
        }
    }

    public sealed class CopyObjectResponse : S3Response
    {
        /// <summary>
        /// Gets the last time this object was modified, as calculated internally and stored by S3.
        /// </summary>
        public DateTime LastModified { get; private set; }

        /// <summary>
        /// Gets the ETag of this object as calculated internally and stored by S3.
        /// </summary>
        public string ETag { get; private set; }

        /// <summary>
        /// Gets the error that occurred during the copy operation, if any.
        /// </summary>
        public S3Exception Error { get; private set; }
        
        protected override void ProcessResponse()
        {
            if (Reader.Name == "Error")
                Error = S3Exception.FromErrorResponse(Reader, null);
            else if (Reader.Name == "CopyObjectResult")
            {
                if (Reader.IsEmptyElement)
                    throw new Exception("Expected a non-empty <CopyObjectResult> element.");

                Reader.ReadStartElement("CopyObjectResult");

                this.LastModified = Reader.ReadElementContentAsDateTime("LastModified", "");
                this.ETag = Reader.ReadElementContentAsString("ETag", "");

                Reader.ReadEndElement();
            }
            else
                throw new Exception("Unknown S3 XML response tag: " + Reader.Name);
        }
    }
}
