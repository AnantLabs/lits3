using System.Net;

namespace LitS3.RestApi
{
    public class AddObjectRequest : S3Request<AddObjectResponse>
    {
        public AddObjectRequest(S3Service service, string bucketName, string objectKey)
            : base(service, "PUT", bucketName, objectKey, null)
        {
            WebRequest.ServicePoint.Expect100Continue = true;
        }
    }

    public class AddObjectResponse : S3Response
    {
        public string ETag { get; private set; }
        public MetadataCollection Metadata { get; private set; }

        protected override void ProcessResponse()
        {
            ETag = WebResponse.Headers[HttpResponseHeader.ETag];
        }
    }
}
