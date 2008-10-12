
using System.IO;
using System;
using System.Net;
namespace LitS3.RestApi
{
    public class GetObjectRequest : S3Request<GetObjectResponse>
    {
        public GetObjectRequest(S3Service service, string bucketName, string key)
            : this(service, bucketName, key, false)
        {
        }

        public GetObjectRequest(S3Service service, string bucketName, string key, bool metadataOnly)
            : base(service, metadataOnly ? "HEAD" : "GET", bucketName, key, null)
        {
        }

        /// <summary>
        ///  Return the object only if it has been modified since the specified time, 
        ///  otherwise return a 304 (not modified).
        /// </summary>
        public DateTime IfModifiedSince
        {
            get { return WebRequest.IfModifiedSince; }
            set { WebRequest.IfModifiedSince = value; }
        }
    }

    public class GetObjectResponse : S3Response
    {
        /// <summary>
        /// Gets the last time this object was modified, as calculated internally and stored by S3.
        /// </summary>
        public DateTime LastModified
        {
            get { return WebResponse.LastModified; }
        }

        /// <summary>
        /// Gets the ETag of this object as calculated internally and stored by S3.
        /// </summary>
        public string ETag
        {
            get { return WebResponse.Headers[HttpResponseHeader.ETag]; }
        }

        /// <summary>
        /// Gets a stream containing the object data (if included).
        /// </summary>
        public Stream GetResponseStream()
        {
            return WebResponse.GetResponseStream();
        }
    }
}