using System.Collections.Generic;
using LitS3.RestApi;

namespace LitS3
{
    public class S3Service
    {
        public string Host { get; set; }
        public bool UseSsl { get; set; }
        public bool UseSubdomains { get; set; }
        public int CustomPort { get; set; }
        public string AccessKeyID { get; set; }
        public string SecretAccessKey { get; set; }
        public string DefaultDelimiter { get; set; }

        public S3Service()
        {
            this.Host = "s3.amazonaws.com";
            this.UseSsl = true;
            this.UseSubdomains = true;
            this.DefaultDelimiter = "/";
        }

        /// <summary>
        /// Lists all buckets owned by you.
        /// </summary>
        public List<Bucket> GetAllBuckets()
        {
            using (GetAllBucketsResponse response = new GetAllBucketsRequest(this).GetResponse())
                return new List<Bucket>(response.Buckets);
        }

        /// <summary>
        /// Creates a bucket in the default storage location automatically determined by Amazon.
        /// </summary>
        /// <param name="bucketName">The name of the bucket, which will be checked against
        /// the BucketNameChecking.Strict requirements.</param>
        public void CreateBucket(string bucketName)
        {
            new CreateBucketRequest(this, bucketName, false).GetResponse().Close();
        }

        /// <summary>
        /// Creates a bucket in the Amazon Europe storage location.
        /// </summary>
        public void CreateBucketInEurope(string bucketName)
        {
            new CreateBucketRequest(this, bucketName, true).GetResponse().Close();
        }

        /// <summary>
        /// Queries S3 about the existance and ownership of the given bucket name.
        /// </summary>
        public BucketAccess QueryBucket(string bucketName)
        {
            try
            {
                // recommended technique from amazon: try and list contents of the bucket with 0 maxkeys
                var args = new ListObjectsArgs { MaxKeys = 0 };
                new ListObjectsRequest(this, bucketName, args).GetResponse().Close();

                return BucketAccess.Accessible;
            }
            catch (S3Exception exception)
            {
                switch (exception.ErrorCode)
                {
                    case S3ErrorCode.NoSuchBucket: return BucketAccess.NoSuchBucket;
                    case S3ErrorCode.AccessDenied: return BucketAccess.NotAccessible;
                    default: throw;
                }
            }
        }

        public bool IsBucketInEurope(string bucketName)
        {
            var request = new GetBucketLocationRequest(this, bucketName);

            using (GetBucketLocationResponse response = request.GetResponse())
                return response.IsEurope;
        }

        public List<ListEntry> ListObjects(string bucketName, string prefix)
        {
            return ListObjects(bucketName, prefix, null);
        }

        public List<ListEntry> ListObjects(string bucketName, string prefix, string marker)
        {
            var args = new ListObjectsArgs { Prefix = prefix, Delimiter = DefaultDelimiter };
            var request = new ListObjectsRequest(this, bucketName, args);

            using (ListObjectsResponse response = request.GetResponse())
                return new List<ListEntry>(response.Entries);
        }

        public void DeleteBucket(string bucketName)
        {
            new DeleteBucketRequest(this, bucketName).GetResponse().Close();
        }

        public void DeleteObject(string bucketName, string key)
        {
            new DeleteObjectRequest(this, bucketName, key).GetResponse().Close();
        }
    }
}
