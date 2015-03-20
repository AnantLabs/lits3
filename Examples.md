## Uploading the contents of a String ##

```
s3.AddObjectString("some simple string content", "MyBucket", "test-object");
```

## Uploading a local file ##

```
s3.AddObject(@"C:\MyFile.txt", "MyBucket", "test-file");
```

## Uploading anything using a Stream ##

```
string objectContents = "This will be written directly to S3.";
long objectLength = objectContents.Length;

s3.AddObject("lits3-demo", "Directly Written.txt", objectLength, stream =>
{
    // Create a StreamWriter to write some text data
    var writer = new StreamWriter(stream, Encoding.ASCII);
    writer.Write(objectContents);
    writer.Flush();
});
```

## Downloading small text files into a String ##

```
Console.WriteLine(s3.GetObjectString("MyBucket", "test-object"));
```

## Downloading from S3 to a local file ##

```
s3.GetObject("MyBucket", "test-file", @"C:\MyFileReturned");
```

## Downloading anything into a Stream ##

```
var request = new GetObjectRequest(s3, "MyBucket", "test-stream");

using (GetObjectResponse response = request.GetResponse())
{
    // At this point you may read the object's Metadata, if any
    Console.WriteLine("Meaning of life: " + response.Metadata["meaning-of-life"]); // prints "42"    

    // Create a StreamReader to read the text data we stored above
    var reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII);
    Console.WriteLine(reader.ReadLine());
}
```

## Downloading only object Metadata ##

```
var metadataOnly = true;
var request = new GetObjectRequest(s3, "MyBucket", "test-stream", metadataOnly);

using (GetObjectResponse response = request.GetResponse())
    Console.WriteLine("Meaning of life: " + response.Metadata["meaning-of-life"]); // prints "42"    
```

## Downloading with progress updates ##

Thanks to Atif, you can use `S3Service.AddObjectProgress` and `S3Service.GetObjectProgress` to be notified when `S3Service` uploads or downloads data for you. Note that this is not threadsafe, so you should construct a new `S3Service` object instance for each of your threads that use it.

```
s3.AddObjectProgress += (s, e) => Console.WriteLine("Progress: " + e.ProgressPercentage);
s3.AddObjectString("Hello world", "lits3-demo", "Test File.txt");

//> Progress: 0
//> Progress: 40
//> ...
//> Progress: 100
```