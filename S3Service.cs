using System;
using System.Collections.Generic;

namespace LitS3
{
    /// <summary>
    /// Provides information about how to connect to an S3 server.
    /// </summary>
    public class S3Service
    {
        /// <summary>
        /// Gets or sets the hostname of the s3 server, usually "s3.amazonaws.com" unless you
        /// are using a 3rd party S3 implementation.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets whether to connect to the server using SSL. The default is true.
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Gets or sets whether to prepend the bucket name as a subdomain when accessing a bucket.
        /// This defaults to true, and is Amazon's preferred method.
        /// </summary>
        public bool UseSubdomains { get; set; }

        /// <summary>
        /// Gets or sets a custom port to use to connect to the S3 server. The default is zero, which
        /// will let this class auto-select the port based on the UseSsl property.
        /// </summary>
        public int CustomPort { get; set; }

        /// <summary>
        /// Gets or sets the Amazon Access Key ID to use for authentication purposes.
        /// </summary>
        public string AccessKeyID { get; set; }

        /// <summary>
        /// Gets or sets the Amazon Secret Access Key to use for authentication purposes.
        /// </summary>
        public string SecretAccessKey { get; set; }

        /// <summary>
        /// Gets or sets the default delimiter to use when calling ListObjects(). The default is
        /// a forward-slash "/".
        /// </summary>
        public string DefaultDelimiter { get; set; }

        /// <summary>
        /// Creates a new S3Service with the default values.
        /// </summary>
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

        /// <summary>
        /// Queries S3 to determine whether the given bucket resides in the Europe location.
        /// </summary>
        public bool IsBucketInEurope(string bucketName)
        {
            var request = new GetBucketLocationRequest(this, bucketName);

            using (GetBucketLocationResponse response = request.GetResponse())
                return response.IsEurope;
        }

        /// <summary>
        /// Queries a bucket for a listing of objects it contains. Only objects with keys
        /// beginning with the given prefix will be returned. The DefaultDelimiter will
        /// be used. This method is for convenience and will throw an exception if the
        /// response was truncated due to too many items.
        /// </summary>
        public List<ListEntry> ListObjects(string bucketName, string prefix)
        {
            var args = new ListObjectsArgs { Prefix = prefix, Delimiter = DefaultDelimiter };
            var request = new ListObjectsRequest(this, bucketName, args);

            using (ListObjectsResponse response = request.GetResponse())
            {
                if (response.IsTruncated)
                    throw new Exception("The server truncated the list of items requested. Consider using the ListObjectsRequest class to query for large numbers of items.");

                return new List<ListEntry>(response.Entries);
            }
        }

        /// <summary>
        /// Deletes the bucket with the given name.
        /// </summary>
        public void DeleteBucket(string bucketName)
        {
            new DeleteBucketRequest(this, bucketName).GetResponse().Close();
        }

        /// <summary>
        /// Deletes the object in the specified bucket with the specified key.
        /// </summary>
        public void DeleteObject(string bucketName, string key)
        {
            new DeleteObjectRequest(this, bucketName, key).GetResponse().Close();
        }
    }
}
