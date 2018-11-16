using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.Ref
{
    public class PackageInfo
    {
        public PackageInfo() { Versions = new List<PackageInfoArtifacts>(); }
        public string Name { get; set; }
        public List<PackageInfoArtifacts> Versions;

        // TODO: add scm revision to package version
        public PackageInfoArtifacts NewRevision(string tag, string dllName, string pdbName, string dllPath)
        {
            PackageInfoArtifact dllInfo = PackageInfoArtifact.Get(dllPath);
            PackageInfoArtifacts v = new PackageInfoArtifacts()
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
        public PackageInfoArtifacts NewRevision(BuildServers.BuildArtifacts artRef)
        {
            BuildServers.BuildArtifact assemart = artRef.GetArtifact(artRef.AssemblyArtefactName);
            PackageInfoArtifacts v = new PackageInfoArtifacts()
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
        public PackageInfoArtifacts FindLatestVersion()
        {
            PackageInfoArtifacts pv = (from v in Versions
                                    orderby v.AddedAt descending
                                    select v).FirstOrDefault();
            return pv;
        }
        public byte[] Serialise()
        {
            string v = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            return Encoding.Unicode.GetBytes(v);
        }
        public static PackageInfo DeSerialise(byte[] data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PackageInfo>(Encoding.Unicode.GetString(data));
        }
    }
}
