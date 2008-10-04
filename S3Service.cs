using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace LitS3
{
    public class S3Service
    {
        const string AmazonHeaderPrefix = "x-amz-";
        const string AmazonDateHeader = "x-amz-date";
        const string MetadataPrefix = "x-amz-meta-";
        const string BucketNameHeader = "x-bucket-name";

        public string Host { get; set; }
        public bool UseSsl { get; set; }
        public bool UseSubdomains { get; set; }
        public int CustomPort { get; set; }
        public string AccessKeyID { get; set; }
        public string SecretAccessKey { get; set; }

        public S3Service()
        {
            this.Host = "s3.amazonaws.com";
            this.UseSsl = true;
            this.UseSubdomains = true;
        }

        public ListAllMyBucketsResult ListAllMyBuckets()
        {
            HttpWebRequest request = CreateWebRequest("GET", null, null);
            AuthorizeRequest(request);

            using (WebResponse response = request.GetResponse())
                return ListAllMyBucketsResult.FromXml(CreateXmlReader(response));
        }

        XmlTextReader CreateXmlReader(WebResponse response)
        {
            var reader = new XmlTextReader(response.GetResponseStream());
            reader.WhitespaceHandling = WhitespaceHandling.Significant;
            reader.Namespaces = false;
            reader.MoveToContent();
            return reader;
        }

        public HttpWebRequest CreateWebRequest(string method, string bucketName, string objectKey)
        {
            var uriString = new StringBuilder(UseSsl ? "https://" : "http://");

            if (bucketName != null && UseSubdomains)
                uriString.Append(bucketName).Append('.');

            uriString.Append(Host);

            if (CustomPort != 0)
                uriString.Append(':').Append(CustomPort);

            uriString.Append('/');

            if (bucketName != null && !UseSubdomains)
                uriString.Append(bucketName).Append('/');

            // could be null
            uriString.Append(objectKey);

            var uri = new Uri(uriString.ToString());

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.AllowWriteStreamBuffering = false;
            request.AllowAutoRedirect = false;
            if (bucketName != null)
                request.Headers[BucketNameHeader] = bucketName;
            return request;
        }

        /// <summary>
        /// Signs the given HttpWebRequest using the HTTP Authorization header with a value
        /// generated using the contents of the request plus our SecretAccessKey.
        /// </summary>
        /// <remarks>
        /// See http://docs.amazonwebservices.com/AmazonS3/2006-03-01/RESTAuthentication.html
        /// 
        /// Needs to be refactored into other classes for reuse in constructing public URLs for objects.
        /// </remarks>
        public void AuthorizeRequest(HttpWebRequest request)
        {
            if (request.Headers[HttpRequestHeader.Authorization] != null)
                throw new ArgumentException("The given request already contains the Authorization header.", "request");

            request.Headers[AmazonDateHeader] = DateTime.UtcNow.ToString("r");

            var stringToSign = new StringBuilder()
                .Append(request.Method).Append('\n')
                .Append(request.Headers[HttpRequestHeader.ContentMd5]).Append('\n')
                .Append(request.ContentType).Append('\n')
                .Append('\n'); // ignore the official Date header since WebRequest won't send it

            var amzHeaders = new SortedList<string, string[]>();

            foreach (string header in request.Headers)
                if (header.StartsWith(AmazonHeaderPrefix))
                    amzHeaders.Add(header.ToLower(), request.Headers.GetValues(header));

            // append the sorted headers in amazon's defined CanonicalizedAmzHeaders format
            foreach (var amzHeader in amzHeaders)
            {
                stringToSign.Append(amzHeader.Key).Append(':');

                // ensure that there's no space around the colon
                bool lastCharWasWhitespace = true;

                foreach (char c in string.Join(",", amzHeader.Value))
                {
                    bool isWhitespace = char.IsWhiteSpace(c);

                    if (isWhitespace && !lastCharWasWhitespace)
                        stringToSign.Append(' '); // amazon wants whitespace "folded" to a single space
                    else if (!isWhitespace)
                        stringToSign.Append(c);

                    lastCharWasWhitespace = isWhitespace;
                }

                stringToSign.Append('\n');
            }

            // append the resource WebRequested using amazon's CanonicalizedResource format

            // does this WebRequest address a bucket?
            string bucketName = request.Headers[BucketNameHeader];

            if (UseSubdomains && bucketName != null)
            {
                stringToSign.Append('/').Append(bucketName);
                request.Headers.Remove(BucketNameHeader);
            }

            stringToSign.Append(request.RequestUri.AbsolutePath);
            
            // todo: add sub-resource, if present. "?acl", "?location", "?logging", or "?torrent"

            // encode
            var signer = new HMACSHA1(Encoding.UTF8.GetBytes(SecretAccessKey));
            var signed = Convert.ToBase64String(signer.ComputeHash(Encoding.UTF8.GetBytes(stringToSign.ToString())));

            string authorization = string.Format("AWS {0}:{1}", AccessKeyID, signed);

            request.Headers[HttpRequestHeader.Authorization] = authorization;
        }
    }
}
