
namespace LitS3
{
    static class S3Headers
    {
        public const string AmazonHeaderPrefix = "x-amz-";
        public const string AmazonDateHeader = "x-amz-date";
        public const string CannedAclHeader = "x-amz-acl";
        public const string MetadataHeaderPrefix = "x-amz-meta-";

        /// <summary>
        /// This is set to the number of metadata entries not returned in x-amz-meta headers.
        /// This can happen if you create metadata using an API like SOAP that supports more
        /// flexible metadata than the REST API. For example, using SOAP, you can create metadata
        /// whose values are not legal HTTP headers.
        /// </summary>
        public const string MissingMetadataHeader = "x-amz-missing-meta";
    }
}
