using FileContentArchive;
using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SourceControl.Containers
{
    public class AssemblyBinVersions
    {
        const string packageInfo = "package.json";
        public ContentVersionStorage versionContainer;
        public string Path { get; private set; }
        public string name { get; private set; }

        public AssemblyBinVersions(string directoryScope, string name)
        {
            this.Path = System.IO.Path.Combine(directoryScope, name + ".zip");
            this.name = name;
            versionContainer = new FileContentArchive.ContentVersionStorage(new FileContentArchive.ZipStorage(Path));
        }

        private PackageInfo getPackageInfo()
        {
            byte[] p = versionContainer.GetRootFileData(packageInfo);
            if (p == null)
                return null;
            return PackageInfo.DeSerialise(p);
        }
        private void setPackageInfo(PackageInfo p)
        {
            byte[] data = p.Serialise();
            versionContainer.SetRootFileData(packageInfo, data);
        }

        public void AddVersion(VersionRevision assemblyRev, Build.AssemblyBinaryBuildResult files)
        {
            PackageInfo p = getPackageInfo();
            if (p == null)
                p = new PackageInfo();

            string revision = assemblyRev.Revision;

            PackageVersion pv = p.AddRevision(revision,
                System.IO.Path.GetFileName(files.LibraryPath),
                files.SymbolsPath == null ? null : System.IO.Path.GetFileName(files.SymbolsPath), files.LibraryPath);

            versionContainer.AddVersionData(revision + "/" + pv.FileLibarary, files.library);
            versionContainer.AddVersionData(revision + "/" + pv.FileSymbols, files.symbols);

            assemblyRev.CreateAt = DateTime.Now;
            byte[] revdata = assemblyRev.Serialise();
            versionContainer.AddVersionData(revision + "/.revision", revdata);
            foreach (var artPath in files.assets)
            {
                string fileName = System.IO.Path.GetFileName(artPath);
                versionContainer.AddVersionData(revision + "/artefacts/" + fileName, System.IO.File.ReadAllBytes(artPath));
                pv.AddArtefact("artefacts/" + fileName, artPath);
            }
            setPackageInfo(p);
        }

        //public bool GetLatestVersion(out string revision, out byte[] library, out byte[] symbols)
        //public bool GetLatestVersion(out string revision, out AssemblyBinaryBuildResult bin)
        public AssemblyVersionPackage GetLatestVersion()
        {
            Ref.PackageInfo pinfo = getPackageInfo();
            if (pinfo == null)
                return null;

            Ref.PackageVersion pv = pinfo.FindLatestVersion();
            AssemblyVersionPackage pckg = new AssemblyVersionPackage(pv, this);
            return pckg;

            //bin = new AssemblyBinary();
            //List<VersionData> files = versionContainer.GetLatestVersion(out revision);
            //if (revision == null)
            //    return false;
            //VersionData vdl = (from f in files
            //                   where !f.Name.Contains("artefacts") && f.Name.EndsWith(".dll")
            //                   select f).First();
            //bin.library = vdl.data;
            //bin.Name = System.IO.Path.GetFileNameWithoutExtension(vdl.Name);
            //bin.symbols = (from f in files
            //               where !f.Name.Contains("artefacts") && f.Name.EndsWith(".pdb")
            //               select f.data).First();
            //foreach (var item in (from f in files
            //                      where f.Name.Contains("artefacts/")
            //                      select f))
            //{
            //    AssemblyAsset at = new AssemblyAsset
            //           {
            //               Data = item.data,
            //               Name = System.IO.Path.GetFileName(item.Name)
            //           };
            //    bin.AddAsset(at);
            //}
            //return true;
        }
        //public bool GetSpecificVersion(string revision, out byte[] library, out byte[] symbols)
        //public bool GetSpecificVersion(string revision, out AssemblyBinaryBuildResult bin)
        //{
        //    bin = new AssemblyBinaryBuildResult();

        //    VersionData[] files = versionContainer.GetSpecificVersion(revision).ToArray();
        //    if (files.Length == 0)
        //        return false;
        //    bin.library = (from f in files
        //                   where !f.Name.Contains("artefacts") && f.Name.EndsWith(".dll")
        //                   select f.data).First();
        //    bin.symbols = (from f in files
        //                   where !f.Name.Contains("artefacts") && f.Name.EndsWith(".pdb")
        //                   select f.data).First();
        //    return true;
        //}
        public bool GetSpecificVersionArtefact(string revision, string artefactName, out Build.BuildResultFile asset)
        {
            asset = new Build.BuildResultFile { Name = artefactName };

            VersionData[] files = versionContainer.GetSpecificVersion(revision).ToArray();
            if (files.Length == 0)
                return false;

            asset.Data = (from f in files
                          where f.Name == artefactName
                          select f.data).First();
            return true;
        }
        public string LatestRevision
        {
            get
            {
                //return versionContainer.key_most_fresh;
                Ref.PackageInfo pinfo = getPackageInfo();
                if (pinfo == null)
                    return null;
                Ref.PackageVersion pv = pinfo.FindLatestVersion();
                return pv.Tag;
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
