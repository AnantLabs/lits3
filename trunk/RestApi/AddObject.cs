using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Cache;

namespace LitS3.RestApi
{
    /// <summary>
    /// Uploads an object to S3.
    /// </summary>
    public class AddObjectRequest : S3Request<AddObjectResponse>
    {
        const string CannedAclHeader = "x-amz-acl";

        // created on demand to save memory
        NameValueCollection metadata;
        bool contentLengthWasSet;

        public AddObjectRequest(S3Service service, string bucketName, string objectKey)
            : base(service, "PUT", bucketName, objectKey, null)
        {
            this.CannedAcl = CannedAcl.Private;
            WebRequest.ServicePoint.Expect100Continue = true;
        }

        /// <summary>
        /// Gets or sets the "canned" access control to apply to this object. The default is
        /// Private. More complex permission sets than the CannedAcl values are allowed, but
        /// you must use SetObjectAclRequest (not currently implemented).
        /// </summary>
        public CannedAcl CannedAcl { get; set; }

        /// <summary>
        /// Gets or sets the optional expiration date of the object. If specified, it will be 
        /// stored by S3 and returned as a standard Expires header when the object is retrieved.
        /// </summary>
        public DateTime? Expires { get; set; }

        /// <summary>
        /// Gets a collection where you can store name/value metadata pairs to be stored along
        /// with this object. Since we are using the REST API, the names and values are limited
        /// to ASCII encoding. Additionally, Amazon imposes a 2k limit on the total HTTP header
        /// size which includes metadata. Note that LitS3 manages adding and removing the special
        /// "x-amz-meta" prefixes for you.
        /// </summary>
        public NameValueCollection Metadata
        {
            get { return metadata ?? (metadata = new NameValueCollection()); }
        }

        public RequestCachePolicy CachePolicy
        {
            get { return WebRequest.CachePolicy; }
            set { WebRequest.CachePolicy = value; }
        }

        /// <summary>
        /// Gets or sets the MIME type of this object. It will be stored by S3 and returned as a
        /// standard Content-Type header when the object is retrieved.
        /// </summary>
        public string ContentType
        {
            get { return WebRequest.ContentType; }
            set { WebRequest.ContentType = value; }
        }

        /// <summary>
        /// Gets or sets the size of the object you are adding. Setting this property is required.
        /// </summary>
        public long ContentLength
        {
            get { return WebRequest.ContentLength; }
            set { WebRequest.ContentLength = value; contentLengthWasSet = true; }
        }

        /// <summary>
        /// Gets or sets the base64 encoded 128-bit MD5 digest of the message (without the headers)
        /// according to RFC 1864.
        /// </summary>
        public string ContentMD5
        {
            get { return WebRequest.Headers[HttpRequestHeader.ContentMd5]; }
            set { WebRequest.Headers[HttpRequestHeader.ContentMd5] = value; }
        }

        /// <summary>
        /// Gets or sets presentational information for the object. It will be stored by S3 and
        /// returned as a standard Content-Disposition header when the object is retrieved.
        /// </summary>
        /// <remarks>
        /// One use of this header is to cause a browser to download this resource as a file attachment
        /// instead of displaying it inline. For that behavior, use a string like:
        /// "Content-disposition: attachment; filename=mytextfile.txt"
        /// </remarks>
        public string ContentDisposition
        {
            get { return WebRequest.Headers["Content-Disposition"]; }
            set { WebRequest.Headers["Content-Disposition"] = value; }
        }

        /// <summary>
        /// Gets or sets the specified encoding of the object data. It will be stored by S3 
        /// and returned as a standard Content-Encoding header when the object is retrieved.
        /// </summary>
        public string ContentEncoding
        {
            get { return WebRequest.Headers[HttpRequestHeader.ContentEncoding]; }
            set { WebRequest.Headers[HttpRequestHeader.ContentEncoding] = value; }
        }

        public override void Authorize()
        {
            // sanity check
            if (!contentLengthWasSet)
                throw new InvalidOperationException("Amazon S3 requires that you specify ContentLength when adding an object.");

            // write canned ACL, if it's not private (which is implied by default)
            switch (CannedAcl)
            {
                case CannedAcl.PublicRead: 
                    WebRequest.Headers[CannedAclHeader] = "public-read"; break;
                case CannedAcl.PublicReadWrite:
                    WebRequest.Headers[CannedAclHeader] = "public-read-write"; break;
                case CannedAcl.AuthenticatedRead:
                    WebRequest.Headers[CannedAclHeader] = "authenticated-read"; break;
            }

            if (Expires.HasValue)
                WebRequest.Headers[HttpRequestHeader.Expires] = Expires.Value.ToUniversalTime().ToString("r");

            if (metadata != null)
                foreach (string key in metadata)
                    foreach (string value in metadata.GetValues(key))
                        WebRequest.Headers.Add("x-amz-meta-" + key, value);

            base.Authorize();
        }
    }

    public class AddObjectResponse : S3Response
    {
        public string ETag { get; private set; }

        protected override void ProcessResponse()
        {
            ETag = WebResponse.Headers[HttpResponseHeader.ETag];
        }
    }
}
