
namespace LitS3.RestApi
{
    public class GetBucketLocationRequest : S3Request<GetBucketLocationResponse>
    {
        public GetBucketLocationRequest(S3Service service, string bucketName)
            : base(service, "GET", bucketName, null, "?location")
        {
        }
    }

    public class GetBucketLocationResponse : S3Response
    {
        public bool IsEurope { get; private set; }

        protected override void ProcessResponse()
        {
            string location = Reader.ReadElementContentAsString("LocationConstraint", "");

            if (location == "EU")
                IsEurope = true;
        }
    }
}
