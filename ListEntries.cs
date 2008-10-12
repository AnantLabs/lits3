using System;
using System.Xml;

namespace LitS3
{
    public abstract class ListEntry
    {
    }

    public class Contents : ListEntry
    {
        public string Key { get; private set; }
        public DateTime LastModified { get; private set; }
        public string ETag { get; private set; }
        public long Size { get; private set; }
        public Identity Owner { get; private set; }

        internal Contents(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                throw new Exception("Expected a non-empty <Contents> element.");

            reader.ReadStartElement("Contents");
            this.Key = reader.ReadElementContentAsString("Key", "");
            this.LastModified = reader.ReadElementContentAsDateTime("LastModified", "");
            this.ETag = reader.ReadElementContentAsString("ETag", "");
            this.Size = reader.ReadElementContentAsLong("Size", "");

            // this tag may be omitted if you don't have permission to view the owner
            if (reader.Name == "Owner")
                this.Owner = new Identity(reader);

            // this element is meaningless
            if (reader.Name == "StorageClass")
                reader.Skip();

            reader.ReadEndElement();
        }

        public override string ToString()
        {
            return string.Format("S3Object \"{0}\"", Key);
        }
    }

    public class CommonPrefix : ListEntry
    {
        public string Prefix { get; private set; }

        internal CommonPrefix(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                throw new Exception("Expected a non-empty <Prefix> element.");

            this.Prefix = reader.ReadElementContentAsString("Prefix", "");
        }

        public override string ToString()
        {
            return string.Format("Common Prefix \"{0}\"", Prefix);
        }
    }
}
