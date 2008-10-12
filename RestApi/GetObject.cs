using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitS3.RestApi
{
    public class GetObjectRequest : S3Request<GetObjectResponse>
    {
        public GetObjectRequest(S3Service service, string bucketName, string key, bool metadataOnly)
            : base(service, metadataOnly ? "HEAD" : "GET", bucketName, key, null)
        {
        }
    }

    public class GetObjectResponse : S3Response
    {
    }
}