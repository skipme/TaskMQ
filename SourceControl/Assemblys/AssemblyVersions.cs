using FileContentArchive;
using SourceControl.Assemblys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl
{
    public class AssemblyVersions
    {
        public ContentVersionStorage versions;
        public string Path { get; private set; }
        public string name { get; private set; }

        public AssemblyVersions(string Path, string name)
        {
            this.Path = Path;
            this.name = name;
            versions = new FileContentArchive.ContentVersionStorage(new FileContentArchive.ZipStorage(Path));
        }

        public void AddVersion(string revision, byte[] library, byte[] pdb)
        {
            revision = string.Format("{0:dd.MM.yy HH-mm} - {1}", DateTime.UtcNow, revision);
            versions.AddVersion(revision + "/" + name + ".dll", library);
            versions.AddVersion(revision + "/" + name + ".pdb", pdb);
        }

        public bool GetLatestVersion(out string revision, out byte[] library, out byte[] symbols)
        {
            revision = versions.key_most_fresh;
            VersionData[] files = versions.GetLatestVersion().ToArray();
            library = (from f in files
                       where f.Name.EndsWith(".dll")
                       select f.data).First();
            symbols = (from f in files
                       where f.Name.EndsWith(".pdb")
                       select f.data).First();
            return true;
        }

        public List<FileStorageEntry> GetVersions()
        {
            return versions.GetVersions();
        }
    }
}
