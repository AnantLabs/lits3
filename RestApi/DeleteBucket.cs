using System;
using System.Net;

namespace LitS3.RestApi
{
    public class DeleteBucketRequest : S3Request<DeleteBucketResponse>
    {
        public DeleteBucketRequest(S3Service service, string bucketName)
            : base(service, "DELETE", bucketName, null, null)
        {
        }
    }

    public class DeleteBucketResponse : S3Response
    {
        protected override void ProcessResponse()
        {
            if (WebResponse.StatusCode != HttpStatusCode.NoContent)
                throw new Exception("Unexpected status code: " + WebResponse.StatusCode);
        }
    }
}
