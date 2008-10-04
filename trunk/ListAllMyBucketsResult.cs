using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace LitS3
{
    public class ListAllMyBucketsResult
    {
        public Identity Owner { get; private set; }
        public ReadOnlyCollection<Bucket> Buckets { get; private set; }

        private ListAllMyBucketsResult() { }

        public static ListAllMyBucketsResult FromXml(XmlReader reader)
        {
            var buckets = new List<Bucket>();
            var result = new ListAllMyBucketsResult();
            result.Buckets = new ReadOnlyCollection<Bucket>(buckets);

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
                        buckets.Add(Bucket.FromXml(reader));
            }
            else throw new Exception("Expected <Buckets>.");

            return result;
        }
    }
}
