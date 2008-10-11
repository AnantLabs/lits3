using System;
using System.Xml;

namespace LitS3
{
    public class Bucket
    {
        public string Name { get; private set; }
        public DateTime CreationDate { get; private set; }

        internal Bucket(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                throw new Exception("Expected a non-empty <Bucket> element.");

            // Example:
            // <Bucket>
            //     <Name>quotes;/Name>
            //     <CreationDate>2006-02-03T16:45:09.000Z</CreationDate>
            // </Bucket>
            reader.ReadStartElement("Bucket");
            this.Name = reader.ReadElementContentAsString("Name", "");
            this.CreationDate = reader.ReadElementContentAsDateTime("CreationDate", "");
            reader.ReadEndElement();
        }

        public override string ToString()
        {
            return string.Format("Bucket \"{0}\"", Name);
        }
    }
}
