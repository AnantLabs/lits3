﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LitS3
{
    /// <summary>
    /// Describes how to connect to a particular S3 server.
    /// </summary>
    public class S3Service
    {
        string secretAccessKey;
        S3Authorizer authorizer;

        internal S3Authorizer Authorizer { get { return authorizer; } }

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
        public string SecretAccessKey
        {
            get { return secretAccessKey; }
            set
            {
                secretAccessKey = value;
                authorizer = new S3Authorizer(this);
            }
        }

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

        #region Basic Operations

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

        #endregion

        #region Public Uri construction

        /// <summary>
        /// This constructs a Uri suitable for accessing the given object in the given bucket.
        /// It is not authorized, so it will only work for objects with anonymous read access.
        /// This method itself does not communicate with S3 and will return immediately.
        /// </summary>
        public string GetUrl(string bucketName, string key)
        {
            var uriString = new StringBuilder();
            uriString.Append("http://");

            if (UseSubdomains)
                uriString.Append(bucketName).Append('.');

            uriString.Append(Host);

            if (CustomPort != 0)
                uriString.Append(':').Append(CustomPort);

            uriString.Append('/');

            if (!UseSubdomains)
                uriString.Append(bucketName).Append('/');

            // EscapeDataString allows keys to have any characters, including "+".
            uriString.Append(Uri.EscapeDataString(key));

            return uriString.ToString();
        }

        /// <summary>
        /// Creates a pre-authorized URI valid for performing a GET on the given S3 object
        /// in the given bucket. This is useful for constructing a URL to hand over to a 3rd party
        /// (such as a web browser). The Uri will automatically expire after the time given.
        /// This method itself does not communicate with S3 and will return immediately.
        /// </summary>
        /// <remarks>
        /// You might expect this method to return a System.Uri instead of a string. It turns out
        /// there is a tricky issue with constructing Uri objects from these pre-authenticated
        /// url strings: The Uri.ToString() method will convert a properly-encoded "+" character back
        /// into a raw "+", which is interpreted by Amazon S3 as a space (standard URI conventions).
        /// So the signature will be misread if you were to take the Uri.ToString() and feed
        /// it to a browser. So instead, we'll give you a properly escaped URL string which 
        /// will always work in a browser. If you want to, say, use it in a WebRequest instead, 
        /// it turns out that WebRequest will leave it escaped properly and everything will work.
        /// </remarks>
        public string GetAuthorizedUrl(string bucketName, string key, DateTime expires)
        {
            string authorization = authorizer.AuthorizeQueryString(bucketName, key, expires);
            
            var uriString = new StringBuilder(GetUrl(bucketName, key))
                .Append("?AWSAccessKeyId=").Append(AccessKeyID)
                .Append("&Expires=").Append(expires.SecondsSinceEpoch())
                .Append("&Signature=").Append(Uri.EscapeDataString(authorization));

            return uriString.ToString();
        }

        #endregion

        #region AddObject and overloads

        /// <summary>
        /// Adds an object to S3 by reading the specified amount of data from the given stream.
        /// </summary>
        public void AddObject(Stream inputStream, long bytes, string bucketName, string key, 
            string contentType, CannedAcl acl)
        {
            var request = new AddObjectRequest(this, bucketName, key);
            request.ContentLength = inputStream.Length;

            if (contentType != null) // if specified
                request.ContentType = contentType;

            if (acl != default(CannedAcl))
                request.CannedAcl = acl;

            using (Stream requestStream = request.GetRequestStream())
                CopyStream(inputStream, requestStream, bytes);

            request.GetResponse().Close();
        }

        /// <summary>
        /// Adds an object to S3 by reading all the data in the given stream. The stream must support
        /// the Length property.
        /// </summary>
        public void AddObject(Stream inputStream, string bucketName, string key,
            string contentType, CannedAcl acl)
        {
            AddObject(inputStream, inputStream.Length, bucketName, key, contentType, acl);
        }

        /// <summary>
        /// Adds an object to S3 by reading all the data in the given stream. The stream must support
        /// the Length property.
        /// </summary>
        public void AddObject(Stream inputStream, string bucketName, string key)
        {
            AddObject(inputStream, inputStream.Length, bucketName, key, null, default(CannedAcl));
        }

        /// <summary>
        /// Uploads the contents of an existing local file to S3.
        /// </summary>
        public void AddObject(string inputFile, string bucketName, string key,
            string contentType, CannedAcl acl)
        {
            using (Stream inputStream = File.OpenRead(inputFile))
                AddObject(inputStream, inputStream.Length, bucketName, key, contentType, acl);
        }

        /// <summary>
        /// Uploads the contents of an existing local file to S3.
        /// </summary>
        public void AddObject(string inputFile, string bucketName, string key)
        {
            AddObject(inputFile, bucketName, key, null, default(CannedAcl));
        }

        #endregion

        #region GetObject and overloads

        /// <summary>
        /// Gets an existing object in S3 and copies its data to the given Stream.
        /// </summary>
        public void GetObject(string bucketName, string key, Stream outputStream, out string contentType)
        {
            var request = new GetObjectRequest(this, bucketName, key);

            using (GetObjectResponse response = request.GetResponse())
            {
                contentType = response.ContentType;
                CopyStream(response.GetResponseStream(), outputStream, response.ContentLength);
            }
        }

        /// <summary>
        /// Gets an existing object in S3 and copies its data to the given Stream.
        /// </summary>
        public void GetObject(string bucketName, string key, Stream outputStream)
        {
            string contentType;
            GetObject(bucketName, key, outputStream, out contentType);
        }

        /// <summary>
        /// Downloads an existing object in S3 to the given local file path.
        /// </summary>
        public void GetObject(string bucketName, string key, string outputFile, out string contentType)
        {
            using (Stream outputStream = File.Create(outputFile))
                GetObject(bucketName, key, outputStream, out contentType);
        }

        /// <summary>
        /// Downloads an existing object in S3 to the given local file path.
        /// </summary>
        public void GetObject(string bucketName, string key, string outputFile)
        {
            string contentType;
            GetObject(bucketName, key, outputFile, out contentType);
        }

        #endregion

        #region CopyStream

        void CopyStream(Stream source, Stream dest, long length)
        {
            var buffer = new byte[8192];

            while (length > 0) // reuse this local var
            {
                int bytesRead = source.Read(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                    dest.Write(buffer, 0, bytesRead);
                else
                    throw new Exception("Unexpected end of stream while copying.");

                length -= bytesRead;
            }
        }

        #endregion
    }
}
