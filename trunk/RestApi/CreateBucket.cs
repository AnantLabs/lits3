using System.IO;
using System.Net;
using System.Text;

namespace LitS3.RestApi
{
    public class CreateBucketRequest : S3Request<CreateBucketResponse>
    {
        const string EuropeConstraint = 
            "<CreateBucketConfiguration><LocationConstraint>EU</LocationConstraint></CreateBucketConfiguration>";

        public CreateBucketRequest(S3Service service, string bucketName, bool createInEurope)
            : base (service, "PUT", bucketName, null, null)
        {
            if (createInEurope)
                WebRequest.ContentLength = EuropeConstraint.Length;
        }

        public void WriteEuropeConstraint(Stream stream)
        {
            var writer = new StreamWriter(stream, Encoding.ASCII);
            writer.Write(EuropeConstraint);
            writer.Flush();
        }

        public override CreateBucketResponse GetResponse()
        {
            // create in europe?
            if (WebRequest.ContentLength > 0)
                using (Stream stream = GetRequestStream())
                    WriteEuropeConstraint(stream);

            return base.GetResponse();
        }
    }

    public class CreateBucketResponse : S3Response
    {
        /// <summary>
        /// The location of the created bucket, as returned by Amazon in the Location header.
        /// </summary>
        public string Location { get; private set; }

        protected override void ProcessResponse()
        {
            Location = WebResponse.Headers[HttpResponseHeader.Location];
        }
    }
}
