using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitS3.RestApi
{
    public class ListBucketRequest
    {
        public string BucketName { get; set; }
        public string Prefix { get; set; }
        public string Marker { get; set; }
        public string Delimiter { get; set; }
        public string MaxKeys { get; set; }
    }
}
