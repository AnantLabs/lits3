using System;
using System.Collections.Generic;
using System.Xml;

namespace LitS3.RestApi
{
    public class ListAllMyBucketsResult
    {
        public Identity Owner { get; private set; }
        public List<Bucket> Buckets { get; private set; }

        private ListAllMyBucketsResult()
        {
            this.Buckets = new List<Bucket>();
        }

        public static ListAllMyBucketsResult FromXml(XmlReader reader)
        {
            var result = new ListAllMyBucketsResult();

            // See http://docs.amazonwebservices.com/AmazonS3/2006-03-01/RESTServiceGET.html

            reader.ReadStartElement("ListAllMyBucketsResult");

            if (reader.Name == "Owner")
                result.Owner = Identity.FromXml(reader);
            else
                throw new Exception("Expected <Owner>.");

            if (reader.Name == "Buckets")
            {
                if (!reader.IsEmptyElement && reader.Read())
                    while (reader.Name == "Bucket")
                        result.Buckets.Add(Bucket.FromXml(reader));
            }
            else throw new Exception("Expected <Buckets>.");

            return result;
        }
    }
}
