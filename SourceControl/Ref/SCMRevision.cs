using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using TaskUniversum;

namespace SourceControl.Ref
{
    public class SCMRevision : IRevision
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
        public static SCMRevision DeSerialise(byte[] data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<SCMRevision>(Encoding.Unicode.GetString(data));
        }
    }
}
