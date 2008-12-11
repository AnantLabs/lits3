using System;
using System.Collections.Generic;

namespace LitS3
{
    /// <summary>
    /// Lists entries in a bucket using multiple requests with no limit.
    /// </summary>
    public class ListEntryReader : IDisposable
    {
        S3Service service;
        string bucketName;
        string prefix;
        string delimiter;
        string marker;
        ListObjectsResponse response;

        public ListEntryReader(S3Service service, string bucketName, string prefix, string delimiter)
        {
            this.service = service;
            this.bucketName = bucketName;
            this.prefix = prefix;
            this.delimiter = delimiter;
        }

        /// <summary>
        /// Provides a forward-only reader for efficiently enumerating through the response
        /// list of objects and common prefixes.
        /// </summary>
        public IEnumerable<ListEntry> Entries
        {
            get
            {
                while (true)
                {
                    var args = new ListObjectsArgs
                    {
                        Prefix = prefix,
                        Delimiter = delimiter,
                        Marker = marker
                    };

                    var request = new ListObjectsRequest(service, bucketName, args);

                    using (response = request.GetResponse())
                    {
                        ListEntry lastEntry = null;

                        foreach (ListEntry entry in response.Entries)
                        {
                            lastEntry = entry;
                            yield return entry;
                        }

                        if (response.IsTruncated)
                        {
                            // if you specified a delimiter, S3 is supposed to give us the marker
                            // name to use in order to get the next set of "stuff".
                            if (response.NextMarker != null)
                                marker = response.NextMarker;
                            // if you didn't specify a delimiter, you won't get any CommonPrefixes,
                            // so we'll use the last ObjectEntry's key as the next delimiter.
                            else if (lastEntry is ObjectEntry)
                                marker = (lastEntry as ObjectEntry).Key;
                            else
                                throw new Exception("S3 Server is misbehaving.");
                        }
                        else
                            break; // we're done!
                    }
                }
            }
        }

        /// <summary>
        /// Closes this reader, which automatically closes the last ListObjects response.
        /// </summary>
        public void Close()
        {
            if (response != null)
                response.Close();
        }

        // explicit implementation so you can use the "using" keyword
        void IDisposable.Dispose()
        {
            Close();
        }
    }
}
