using FileContentArchive;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SourceControl.Assemblys
{
    public class AssemblyBinVersions
    {
        public ContentVersionStorage versionContainer;
        public string Path { get; private set; }
        public string name { get; private set; }

        public AssemblyBinVersions(string dirContainer, string name)
        {
            this.Path = System.IO.Path.Combine(dirContainer, name + ".zip");
            this.name = name;
            versionContainer = new FileContentArchive.ContentVersionStorage(new FileContentArchive.ZipStorage(Path));
        }

        public void AddVersion(VersionRevision assemblyRev, byte[] library, byte[] pdb, IEnumerable<BuildResultFile> artefacts)
        {
            string revision = assemblyRev.Revision;
            //revision = string.Format("{0:dd.MM.yy HH-mm} ${1}", DateTime.UtcNow, revision);
            versionContainer.AddVersionData(revision + "/" + name + ".dll", library);
            versionContainer.AddVersionData(revision + "/" + name + ".pdb", pdb);

            assemblyRev.CreateAt = DateTime.Now;
            byte[] revdata = assemblyRev.Serialise();
            versionContainer.AddVersionData(revision + "/.revision", revdata);
            foreach (var art in artefacts)
            {
                versionContainer.AddVersionData(revision + "/artefacts/" + art.Name, art.Data);
            }
        }

        public bool GetLatestVersion(out string revision, out byte[] library, out byte[] symbols)
        {
            revision = null;
            library = symbols = null;
            List<VersionData> files = versionContainer.GetLatestVersion(out revision);
            if (revision == null)
                return false;
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

            VersionData[] files = versionContainer.GetSpecificVersion(revision).ToArray();
            if (files.Length == 0)
                return false;
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
        public List<VersionRevision> GetVersions()
        {
            List<VersionRevision> vers = new List<VersionRevision>();
            foreach (string f in versionContainer.GetVersions())
            {
                VersionData vd = versionContainer.GetSpecificVersionData(f + "/.revision");
                if (vd == null)
                {
                    Console.WriteLine("binary version {0} unannotated with revision data in {1}", f, Path);
                    continue;
                }
                VersionRevision vr = VersionRevision.DeSerialise(vd.data);
                vr.CreateAt = vd.Created;
                vers.Add(vr);
            }
            return vers;
        }
    }
}
