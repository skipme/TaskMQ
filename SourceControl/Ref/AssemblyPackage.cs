using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.Ref
{
    public class AssemblyPackage
    {
        public AssemblyPackage() { Versions = new List<AssemblyArtifacts>(); }
        public string Name { get; set; }
        public List<AssemblyArtifacts> Versions;

        // TODO: add scm revision to package version
        public AssemblyArtifacts AddRevision(string tag, string dllName, string pdbName, string dllPath)
        {
            AssemblyArtifact dllInfo = AssemblyArtifact.Get(dllPath);
            AssemblyArtifacts v = new AssemblyArtifacts()
            {
                VersionTag = tag,
                FileLibarary = dllName,
                FileSymbols = pdbName,
                AssemblyVersion = dllInfo.Version,
                AddedAt = DateTime.UtcNow
            };
            Name = dllInfo.Name;
            Versions.Add(v);
            return v;
        }
        public AssemblyArtifacts AddRevision(BuildServers.BuildArtifacts artRef)
        {
            BuildServers.BuildArtifact assemart = artRef.GetArtifact(artRef.AssemblyArtefactName);
            AssemblyArtifacts v = new AssemblyArtifacts()
            {
                VersionTag = artRef.VersionTag,
                FileLibarary = artRef.AssemblyArtefactName,
                FileSymbols = artRef.AssemblyArtefactNameSym,
                AssemblyVersion = assemart.Version,
                AddedAt = DateTime.UtcNow
            };
            Name = artRef.AssemblyArtefactName;
            Versions.Add(v);
            return v;
        }
        public AssemblyArtifacts FindLatestVersion()
        {
            AssemblyArtifacts pv = (from v in Versions
                                    orderby v.AddedAt descending
                                    select v).FirstOrDefault();
            return pv;
        }
        public byte[] Serialise()
        {
            string v = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            return Encoding.Unicode.GetBytes(v);
        }
        public static AssemblyPackage DeSerialise(byte[] data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AssemblyPackage>(Encoding.Unicode.GetString(data));
        }
    }
}
