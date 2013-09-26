using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SourceControl.Ref
{
    public class VersionRevision
    {
        public string Commiter { get; set; }
        public string Revision { get; set; }
        public string CommitMessage { get; set; }
        public DateTime CommitTime { get; set; }
        public DateTime CreateAt { get; set; }

        public byte[] Serialise()
        {
            string v = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            return Encoding.Unicode.GetBytes(v);
        }
        public static VersionRevision DeSerialise(byte[] data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<VersionRevision>(Encoding.Unicode.GetString(data));
        }
        //public byte[] Serialise()
        //{
        //    XmlSerializer xs = new XmlSerializer(typeof(VersionRevision));
        //    using (MemoryStream memoryStream = new MemoryStream())
        //    {
        //        XmlWriterSettings settings = new XmlWriterSettings { Encoding = Encoding.Unicode, Indent = true };
        //        using (XmlWriter xmlTextWriter = XmlWriter.Create(memoryStream, settings))
        //        {
        //            xs.Serialize(xmlTextWriter, this);
        //        }
        //        return memoryStream.GetBuffer();
        //    }
        //}
        //public static VersionRevision DeSerialise(byte[] data)
        //{
        //    XmlSerializer xs = new XmlSerializer(typeof(VersionRevision));
        //    using (MemoryStream memoryStream = new MemoryStream(data))
        //    {
        //        return xs.Deserialize(memoryStream) as VersionRevision;
        //    }
        //}
    }
}
