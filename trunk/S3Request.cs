using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace LitS3
{
    public class S3Request<TResponse>
        where TResponse : S3Response, new()
    {
        const string AmazonHeaderPrefix = "x-amz-";
        const string AmazonDateHeader = "x-amz-date";
        const string MetadataPrefix = "x-amz-meta-";
        const string BucketNameHeader = "x-bucket-name";

        string bucketName; // remember this for signing the request later

        public S3Service Service { get; private set; }
        public HttpWebRequest WebRequest { get; private set; }

        public S3Request(S3Service service, string method, string bucketName, string objectKey,
            string queryString)
        {
            this.Service = service;
            this.bucketName = bucketName;
            this.WebRequest = CreateWebRequest(method, objectKey, queryString);
        }

        HttpWebRequest CreateWebRequest(string method, string objectKey, string queryString)
        {
            var uriString = new StringBuilder(Service.UseSsl ? "https://" : "http://");

            if (bucketName != null && Service.UseSubdomains)
                uriString.Append(bucketName).Append('.');

            uriString.Append(Service.Host);

            if (Service.CustomPort != 0)
                uriString.Append(':').Append(Service.CustomPort);

            uriString.Append('/');

            if (bucketName != null && !Service.UseSubdomains)
                uriString.Append(bucketName).Append('/');

            // could be null
            uriString.Append(objectKey);

            // could be null
            uriString.Append(queryString);

            var uri = new Uri(uriString.ToString());

            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(uri);
            request.Method = method;
            request.AllowWriteStreamBuffering = false;
            request.AllowAutoRedirect = true;
            return request;
        }

        public bool IsAuthorized
        {
            get { return WebRequest.Headers[HttpRequestHeader.Authorization] != null; }
        }

        void AuthorizeIfNecessary()
        {
            if (!IsAuthorized) Authorize();
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
        public void Authorize()
        {
            if (IsAuthorized)
                throw new InvalidOperationException("This request has already been authorized.");

            WebRequest.Headers[AmazonDateHeader] = DateTime.UtcNow.ToString("r");

            var stringToSign = new StringBuilder()
                .Append(WebRequest.Method).Append('\n')
                .Append(WebRequest.Headers[HttpRequestHeader.ContentMd5]).Append('\n')
                .Append(WebRequest.ContentType).Append('\n')
                .Append('\n'); // ignore the official Date header since WebRequest won't send it

            var amzHeaders = new SortedList<string, string[]>();

            foreach (string header in WebRequest.Headers)
                if (header.StartsWith(AmazonHeaderPrefix))
                    amzHeaders.Add(header.ToLower(), WebRequest.Headers.GetValues(header));

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

            // does this request address a bucket?
            if (Service.UseSubdomains && bucketName != null)
            {
                stringToSign.Append('/').Append(bucketName);
                WebRequest.Headers.Remove(BucketNameHeader);
            }

            stringToSign.Append(WebRequest.RequestUri.AbsolutePath);

            // add sub-resource, if present. "?acl", "?location", "?logging", or "?torrent"
            string query = WebRequest.RequestUri.Query;

            if (query == "?acl" || query == "?location" || query == "?logging" || query == "?torrent")
                stringToSign.Append(query);

            // encode
            var signer = new HMACSHA1(Encoding.UTF8.GetBytes(Service.SecretAccessKey));
            var signed = Convert.ToBase64String(signer.ComputeHash(Encoding.UTF8.GetBytes(stringToSign.ToString())));

            string authorization = string.Format("AWS {0}:{1}", Service.AccessKeyID, signed);

            WebRequest.Headers[HttpRequestHeader.Authorization] = authorization;
        }

        void TryThrowS3Exception(WebException exception)
        {
            // if this is a protocol error and the response type is XML, we can expect that
            // S3 sent us an <Error> message.
            if (exception.Status == WebExceptionStatus.ProtocolError &&
                exception.Response.ContentType == "application/xml")
            {
                var wrapped = S3Exception.FromWebException(exception);
                throw wrapped; // do this on a separate statement so the debugger can re-execute
            }
        }

        /// <summary>
        /// Gets the S3 REST response synchronously. It also calls Authorize() if necessary.
        /// </summary>
        public virtual TResponse GetResponse()
        {
            AuthorizeIfNecessary();

            try
            {
                return new TResponse { WebResponse = (HttpWebResponse)WebRequest.GetResponse() };
            }
            catch (WebException exception)
            {
                TryThrowS3Exception(exception);
                throw;
            }
        }

        /// <summary>
        /// Submits the request to the server and retrieves a Stream for writing body content to.
        /// </summary>
        public virtual Stream GetRequestStream()
        {
            AuthorizeIfNecessary();
            return WebRequest.GetRequestStream();
        }

        /// <summary>
        /// Gets the S3 REST request stream asynchronously. It also calls Authorize() if
        /// necessary.
        /// </summary>
        public virtual IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            AuthorizeIfNecessary();
            return WebRequest.BeginGetRequestStream(callback, state);
        }

        /// <summary>
        /// Ends an asynchronous call to BeginGetRequestStream(). 
        /// </summary>
        public virtual Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return WebRequest.EndGetRequestStream(asyncResult);
        }

        /// <summary>
        /// Gets the S3 REST response asynchronously. It also calls Authorize() if
        /// necessary.
        /// </summary>
        public virtual IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            AuthorizeIfNecessary();
            return WebRequest.BeginGetResponse(callback, state);
        }

        /// <summary>
        /// Ends an asynchronous call to BeginGetResponse().
        /// </summary>
        public virtual TResponse EndGetResponse(IAsyncResult asyncResult)
        {
            try
            {
                return new TResponse { WebResponse = (HttpWebResponse)WebRequest.EndGetResponse(asyncResult) };
            }
            catch (WebException exception)
            {
                TryThrowS3Exception(exception);
                throw;
            }
        }
    }
}
