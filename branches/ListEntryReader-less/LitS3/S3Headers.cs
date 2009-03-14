
namespace LitS3
{
    static class S3Headers
    {
        public const string AmazonHeaderPrefix = "x-amz-";
        public const string AmazonDate = "x-amz-date";
        public const string CannedAcl = "x-amz-acl";
        public const string MetadataPrefix = "x-amz-meta-";
        public const string CopySource = "x-amz-copy-source";

        /// <summary>
        /// This is set to the number of metadata entries not returned in x-amz-meta headers.
        /// This can happen if you create metadata using an API like SOAP that supports more
        /// flexible metadata than the REST API. For example, using SOAP, you can create metadata
        /// whose values are not legal HTTP headers.
        /// </summary>
        public const string MissingMetadata = "x-amz-missing-meta";
    }
}
