using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LitS3.Tests.Properties;
using System.Net;
using System.IO;

namespace LitS3.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            if (string.IsNullOrEmpty(Settings.Default.AccessKeyID) ||
                string.IsNullOrEmpty(Settings.Default.SecretAccessKey))
            {
                Console.WriteLine("You need to edit the LitS3.Tests.exe.config file and enter your S3 login information to run these tests.");
            }
            else
            {
                RunS3Tests();
            }

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        static void RunS3Tests()
        {
            var s3 = new S3Service()
            {
                AccessKeyID = Settings.Default.AccessKeyID,
                SecretAccessKey = Settings.Default.SecretAccessKey
            };

            List<Bucket> buckets = s3.GetAllBuckets();

            buckets.ForEach(bucket => Console.WriteLine(bucket.Name));

            #region Some testing code that needs to be refactored and separated into classes

            //s3.AddObjectProgress += (s, e) => Console.WriteLine("Progress: " + e.ProgressPercentage);
            //s3.AddObjectString("Hello world", "ctu-test", "test-progress");

            //s3.CreateBucketInEurope("test-europe234234");

            //var copyRequest = new CopyObjectRequest(s3, "spotlightmobile", "office.jpg", "office2.jpg");
            //copyRequest.GetResponse().Close();

            //s3.CopyObject("spotlightmobile", "office.jpg", "office2.jpg");

            /*

            string testBucket = "ctu-test";

            // Upload dynamically from a stream
            s3.AddObject(testBucket, "test-stream", 10, stream =>
            {
                // write 10 ASCII characters starting with "a"
                foreach (byte i in Enumerable.Range(65, 10))
                    stream.WriteByte(i);
            });

            // Download dynamically from a stream
            using (Stream stream = s3.GetObjectStream(testBucket, "test-stream"))
                Console.WriteLine("Contents: " + new StreamReader(stream).ReadToEnd());
            
            // List all objects
            s3.ListAllObjects(testBucket, null, entry => Console.WriteLine("Found: " + entry));

            
            //string testBucket = "ctu-test";
            //string testKey = "hello";

            foreach (ListEntry entry in s3.ListObjects("ctu-beta", null))
                Console.WriteLine(entry);

            Console.WriteLine("Bucket status of {0}: {1}", testBucket, s3.QueryBucket(testBucket));

            //s3.DeleteBucket(testBucket);

            s3.AddObjectString("four thousand years", testBucket, testKey, "text/plain", CannedAcl.Private);

            Console.WriteLine(s3.GetObjectString(testBucket, testKey));

            Console.WriteLine(s3.GetUrl(testBucket, testKey));

            Console.WriteLine(s3.GetAuthorizedUrl(testBucket, testKey, DateTime.Now + TimeSpan.FromMinutes(10)));

            //s3.DeleteObject("ctu-test", "hello");

            s3.AddObjectString("jackdaws quartz", testBucket, "jackdaws quartz", "text/plain", CannedAcl.Private);
            s3.AddObjectString("one+two=three", testBucket, "one+two=three", "text/plain", CannedAcl.Private);

            s3.UseSubdomains = false;
            s3.ListObjects("testing_special_chars", null).ForEach(i => Console.WriteLine(i));

            Console.WriteLine(s3.GetObjectString(testBucket, "jackdaws quartz"));
            Console.WriteLine(s3.GetObjectString(testBucket, "one+two=three"));

            Debug.WriteLine(s3.GetAuthorizedUrl(testBucket, "jackdaws quartz", DateTime.Now + TimeSpan.FromMinutes(10)));
            Debug.WriteLine(s3.GetAuthorizedUrl(testBucket, "one+two=three", DateTime.Now + TimeSpan.FromMinutes(10)));

            

            {
                // Create a file on S3 from the contents of a string
                s3.AddObjectString("some simple string content", testBucket, "test-object");

                // Upload a local file 
                //s3.AddObject(@"H:\Music\Andrew Bird\The Swimming Hour\02 - Andrew Bird - The Swimming Hour - Core And Rind.mp3", testBucket, "andrew-bird.mp3");

                // Upload dynamically from a stream
                var request = new AddObjectRequest(s3, testBucket, "test-stream");
                request.ContentLength = 36;

                // Add some metadata
                request.Metadata["meaning-of-life"] = "42";

                // This will call out to the S3 server and initiate an upload
                using (Stream requestStream = request.GetRequestStream())
                {
                    // Create a StreamWriter to write some text data
                    var writer = new StreamWriter(requestStream, Encoding.ASCII);
                    writer.Write("This will be written directly to S3.");
                    writer.Flush();
                }

                // We're finished, so get the response to finish our submission. Remember to Close() it!
                request.GetResponse().Close();
            }

            {
                // Get the contents of a file on S3 as a string
                Console.WriteLine(s3.GetObjectString(testBucket, "test-object"));

                // Download a file from S3 into a local file
                //s3.GetObject(testBucket, "andrew-bird.mp3", @"C:\andrew-bird.mp3");

                // Download dynamically into a stream
                var request = new GetObjectRequest(s3, testBucket, "test-stream");

                using (GetObjectResponse response = request.GetResponse())
                {
                    // Read some metadata
                    Console.WriteLine("Meaning of life: " + response.Metadata["meaning-of-life"]); // prints "42"

                    // Create a StreamReader to read the text data we stored above
                    var reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII);
                    Console.WriteLine(reader.ReadLine());
                }
            }

            {
                // Download dynamically into a stream
                var request = new GetObjectRequest(s3, testBucket, "andrew-bird.mp3");

                using (GetObjectResponse response = request.GetResponse())
                {
                    var buffer = new byte[31768];
                    var bytesDownloaded = 0;
                    var responseStream = response.GetResponseStream();

                    while (bytesDownloaded < response.ContentLength)
                    {
                        var bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                        
                        // write the downloaded data somewhere...
                        
                        bytesDownloaded += bytesRead;

                        var percent = (int)(((float)bytesDownloaded / (float)response.ContentLength) * 100);

                        Console.CursorLeft = 0;
                        Console.Write("Downloading... {0}%", percent);
                    }
                }
            }
            */

            //s3.CreateBucket("ctu-america");
            //Console.WriteLine("America in europe? " + s3.IsBucketInEurope("ctu-america"));

            //s3.CreateBucketInEurope("ctu-europe");
            //Console.WriteLine("Europe in europe? " + s3.IsBucketInEurope("ctu-europe"));

            /*var request = new GetAllBucketsRequest(s3);

            request.BeginGetResponse(delegate (IAsyncResult result)
            {
                using (GetAllBucketsResponse response = request.EndGetResponse(result))
                {
                    foreach (Bucket bucket in response.Buckets)
                        Console.WriteLine(bucket);
                }

            }, null);*/

            #endregion
        }
    }
}
