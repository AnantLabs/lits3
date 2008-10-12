using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;

namespace LitS3.RestApi
{
    public class ListObjectsArgs
    {
        public string Prefix { get; set; }
        public string Marker { get; set; }
        public string Delimiter { get; set; }
        public int? MaxKeys { get; set; }

        public string ToQueryString()
        {
            var builder = new StringBuilder();
            var sep = '?';

            if (!string.IsNullOrEmpty(Prefix))
            { builder.Append(sep).Append("prefix=").Append(HttpUtility.UrlEncode(Prefix)); sep = '&'; }

            if (!string.IsNullOrEmpty(Marker))
            { builder.Append(sep).Append("marker=").Append(HttpUtility.UrlEncode(Marker)); sep = '&'; }

            if (!string.IsNullOrEmpty(Delimiter))
            { builder.Append(sep).Append("delimiter=").Append(HttpUtility.UrlEncode(Delimiter)); sep = '&'; }

            if (MaxKeys.HasValue)
            { builder.Append(sep).Append("max-keys=").Append(MaxKeys.Value); sep = '&'; }

            return builder.ToString();
        }
    }

    public class ListObjectsRequest : S3Request<ListObjectsResponse>
    {
        public string BucketName { get; private set; }
        public ListObjectsArgs Args { get; private set; }

        public ListObjectsRequest(S3Service service, string bucketName, ListObjectsArgs args)
            : base(service, "GET", bucketName, null, args != null ? args.ToQueryString() : null)
        {
            this.BucketName = bucketName;
        }
    }

    public class ListObjectsResponse : S3Response
    {
        public string BucketName { get; private set; }
        public string Prefix { get; private set; }
        public string Marker { get; private set; }
        public int MaxKeys { get; private set; }
        public string Delimiter { get; private set; }
        public bool IsTruncated { get; private set; }
        public string NextMarker { get; private set; }

        protected override void ProcessResponse()
        {
            // See http://docs.amazonwebservices.com/AmazonS3/2006-03-01/ListingKeysResponse.html

            Reader.ReadStartElement("ListBucketResult");

            // the response echoes back the request parameters, read those first in an assumed order
            this.BucketName = Reader.ReadElementContentAsString("Name", "");
            this.Prefix = Reader.ReadElementContentAsString("Prefix", "");
            this.Marker = Reader.ReadElementContentAsString("Marker", "");

            // this is optional
            if (Reader.Name == "NextMarker")
                this.NextMarker = Reader.ReadElementContentAsString("NextMarker", "");

            this.MaxKeys = Reader.ReadElementContentAsInt("MaxKeys", "");

            // this is optional
            if (Reader.Name == "Delimiter")
                this.Delimiter = Reader.ReadElementContentAsString("Delimiter", "");
            
            this.IsTruncated = Reader.ReadElementContentAsBoolean("IsTruncated", "");
        }

        /// <summary>
        /// Provides a forward-only reader for efficiently enumerating through the response
        /// list of objects and common prefixes.
        /// </summary>
        public IEnumerable<ListEntry> Entries
        {
            get
            {
                while (!Reader.IsEmptyElement && Reader.Name == "Contents")
                    yield return new S3Object(Reader);

                while (Reader.Name == "CommonPrefixes" && Reader.Read())
                    while (Reader.Name == "Prefix")
                        yield return new CommonPrefix(Reader);
            }
        }
    }
}
