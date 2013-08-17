using FileContentArchive;
using SourceControl.Assemblys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.Assemblys
{
    public class AssemblyBinVersions
    {
        public class AssemblyRevision
        {
            public string Revision { get; set; }
            public DateTime CreateAt { get; set; }
        }
        public ContentVersionStorage versionContainer;
        public string Path { get; private set; }
        public string name { get; private set; }

        public AssemblyBinVersions(string dirContainer, string name)
        {
            this.Path = System.IO.Path.Combine(dirContainer, name + ".zip");
            this.name = name;
            versionContainer = new FileContentArchive.ContentVersionStorage(new FileContentArchive.ZipStorage(Path));
        }

        public void AddVersion(string revision, byte[] library, byte[] pdb)
        {
            //revision = string.Format("{0:dd.MM.yy HH-mm} ${1}", DateTime.UtcNow, revision);
            versionContainer.AddVersion(revision + "/" + name + ".dll", library);
            versionContainer.AddVersion(revision + "/" + name + ".pdb", pdb);
        }

        public bool GetLatestVersion(out string revision, out byte[] library, out byte[] symbols)
        {
            revision = null;
            library = symbols = null;
            if (revision == null)
                return false;
            List<VersionData> files = versionContainer.GetLatestVersion(out revision);
            library = (from f in files
                       where f.Name.EndsWith(".dll")
                       select f.data).First();
            symbols = (from f in files
                       where f.Name.EndsWith(".pdb")
                       select f.data).First();
            return true;
        }
        public bool GetSpecificVersion(string revision, out byte[] library, out byte[] symbols)
        {
            library = symbols = null;
            if (revision == null)
                return false;
            VersionData[] files = versionContainer.GetSpecificVersion(revision).ToArray();
            library = (from f in files
                       where f.Name.EndsWith(".dll")
                       select f.data).First();
            symbols = (from f in files
                       where f.Name.EndsWith(".pdb")
                       select f.data).First();
            return true;
        }
        public string LatestRevision
        {
            get
            {
                return versionContainer.key_most_fresh;
            }
        }
        public AssemblyRevision[] GetVersions()
        {
            return (from v in versionContainer.GetVersions()
                    where v.IsDir
                    select new AssemblyRevision
                    {
                        CreateAt = v.Created,
                        Revision = v.Location
                    }).ToArray();
        }
    }
}
