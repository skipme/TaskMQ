using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SourceControl
{
    public class VersionRevision
    {
        public string Commiter { get; set; }
        public string Revision { get; set; }
        public string CommitMessage { get; set; }
        public DateTimeOffset CommitTime { get; set; }
        public DateTime CreateAt { get; set; }

        public byte[] Serialise()
        {
            XmlSerializer xs = new XmlSerializer(typeof(VersionRevision));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.Unicode))
                {
                    xs.Serialize(xmlTextWriter, this);
                }
                return memoryStream.GetBuffer();
            }
        }
        public static VersionRevision DeSerialise(byte[] data)
        {
            XmlSerializer xs = new XmlSerializer(typeof(VersionRevision));
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                return xs.Deserialize(memoryStream) as VersionRevision;
            }
        }
    }
}
