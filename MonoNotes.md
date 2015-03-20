# Certificates #

Mono does not ship with any trusted roots, so talking to amazon's S3 servers over HTTPS will immediately fail. See [mozroots](http://manpages.ubuntu.com/manpages/hardy/man1/mozroots.html) for more information on how to address this.

# Connections #

The HTTP standard dictates that only 2 connections may be opened to any one particular server at a time, to help with server load. If you are running LitS3 on your webserver you may hit this limit VERY quickly if you are frequently uploading or downloading large files to S3.

Additionally, at the time of this writing, there is a bug in Mono where aborted connections are counted against this limit, meaning you'll "hit a wall" eventually.

I recommend this workaround:

```
var service = new S3Service { ... };

service.BeforeAuthorize += (sender, e) =>
{
    e.Request.ServicePoint.ConnectionLimit = int.MaxValue;
};
```

This should ensure that your S3Service is allowed to make as many connections as it wants. Amazon S3 is designed to handle a lot more simultaneous connections than you'll ever be able to create, so don't feel bad about it!