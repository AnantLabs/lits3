using System;
using System.Collections.Generic;
using System.Net;
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Queries S3 about the existance and ownership of the given bucket name.
        /// </summary>
        public BucketAccess FindBucket(string bucketName)
        {
            throw new NotImplementedException();
        }

        public void ListBucketContents(string bucketName, string prefix)
        {
            var args = new ListObjectsArgs { Prefix = prefix };
            var request = new ListObjectsRequest(this, bucketName, args);

            //using (HttpWebResponse response = request.GetResponse())
                
        }

        public void ListBucketContents(string bucketName, string prefix, string marker)
        {
            throw new NotImplementedException();
        }
    }
}
