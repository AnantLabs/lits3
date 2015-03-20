LitS3 is a library written in C# that provides comprehensive and straightforward access to Amazon S3 for .NET developers.

## Notably ##

  * Gives you typesafe objects instead of raw XML
  * Well-documented, highly optimized code
  * Uses the more efficient REST API
  * Currently supports most S3 features
  * Works in Mono+MS (see MonoNotes)

## S3Service: A one-liner API ##

```
var s3 = new S3Service
{
    AccessKeyID = Settings.Default.AccessKeyID,
    SecretAccessKey = Settings.Default.SecretAccessKey
};

s3.ForEachBucket(Console.WriteLine);

//> Bucket "mybucket"
//> Bucket "myotherbucket"
//> Bucket "lits3-demo"

s3.AddObjectString("This is file one!", "lits3-demo", "File 1.txt");

s3.ForEachObject("lits3-demo", Console.WriteLine);

//> S3Object "File 1.txt"
//> Common Prefix "MyDirectory"

Console.WriteLine(s3.GetObjectString("lits3-demo", "File 1.txt"));

//> This is file one!

s3.CopyObject("lits3-demo", "File 1.txt", "File 1 copy.txt");

s3.ForEachObject("lits3-demo", Console.WriteLine);

//> S3Object "File 1 copy.txt"
//> S3Object "File 1.txt"
//> Common Prefix "MyDirectory"

s3.ForEachObject("lits3-demo", "MyDirectory/", Console.WriteLine);

//> S3Object "Other File.txt"
```

What's really cool about these examples is that LitS3 is managing memory very efficiently for you. The `S3Service.ForEach…` methods use callbacks to ensure that the actual web requests happen transparently in the background and are properly cleaned up.

For instance, when you call `ForEachObject` to list the contents of a bucket with a million objects, LitS3 will automatically handle the multiple S3 requests necessary to do this, as you iterate through each object. LitS3 will not just construct an array with a million entries!

## Need more flexibility? ##

Every method in S3Service is backed by a corresponding `S3Request`+`S3Response` class pair that, together, encapsulate a normal `WebRequest`+`WebResponse`.

This means that every S3 operation can be performed using `WebRequest`'s asynchronous API:

```
var request = new GetObjectRequest(s3, "lits3-demo", "File 1.txt");

request.BeginGetResponse(result =>
{
    // comes in on a separate thread
    using (GetObjectResponse response = request.EndGetResponse(result))
    {
        StreamReader reader = new StreamReader(response.GetResponseStream());
        Console.WriteLine(reader.ReadToEnd());

        //> This is file one!
    }
}, null);

// continues immediately without blocking...
```

## I don't see (…random S3 feature…) in `S3Service`, can you add it? ##

This is where the `S3Request`+`S3Response` classes mentioned above come in. These backing classes like `AddObjectRequest`, `GetObjectRequest`, etc. expose nearly 100% of S3's API.

For instance, use `AddObjectRequest.Metadata` and `GetObjectResponse.Metadata` if you want to get or set S3 key-value "Metadata" on your objects.

### [More Code Examples](http://code.google.com/p/lits3/wiki/Examples) ###

## What's missing ##

  * Doesn't currently support get/set of non-canned ACLs
  * Doesn't currently support get/set of server access logging
  * Haven't looked at the BitTorrent feature yet, probably trivial