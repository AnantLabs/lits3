using System;
using System.Xml;

namespace LitS3
{
    public class Bucket
    {
        public string Name { get; private set; }
        public DateTime CreationDate { get; private set; }

        private Bucket() { }

        public static Bucket FromXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return null;

            var bucket = new Bucket();

            // Example:
            // <Bucket>
            //     <Name>quotes;/Name>
            //     <CreationDate>2006-02-03T16:45:09.000Z</CreationDate>
            // </Bucket>
            reader.ReadStartElement("Bucket");
            bucket.Name = reader.ReadElementContentAsString("Name", "");
            bucket.CreationDate = reader.ReadElementContentAsDateTime("CreationDate", "");
            reader.ReadEndElement();

            return bucket;
        }
    }
}
