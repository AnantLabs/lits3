using System.Xml;

namespace LitS3
{
    public class Identity
    {
        public string ID { get; private set; }
        public string DisplayName { get; private set; }

        private Identity() { }

        public static Identity FromXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return null;

            var identity = new Identity();

            // Example:
            // <Owner>
            //     <ID>bcaf1ffd86f41caff1a493dc2ad8c2c281e37522a640e161ca5fb16fd081034f</ID>
            //     <DisplayName>webfile</DisplayName>
            // </Owner>
            reader.ReadStartElement("Owner");
            identity.ID = reader.ReadElementContentAsString("ID", "");
            identity.DisplayName = reader.ReadElementContentAsString("DisplayName", "");
            reader.ReadEndElement();

            return identity;
        }
    }
}
