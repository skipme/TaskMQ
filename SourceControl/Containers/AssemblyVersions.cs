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

        public DateTime LastPackagedDate
        {
            get
            {
                return GetLatestVersion().Version.AddedAt;
            }
        }
        public AssemblyPackage PackageInfo { get; private set; }

        public AssemblyBinVersions(string directoryScope, string name)
        {
            this.Path = System.IO.Path.Combine(directoryScope, name + ".zip");
            this.name = name;
            versionContainer = new FileContentArchive.ContentVersionStorage(new FileContentArchive.ZipStorage(Path));

            PackageInfo = getPackageInfo();
        }

        private AssemblyPackage getPackageInfo()
        {
            byte[] p = versionContainer.GetRootFileData(packageInfo);
            if (p == null)
                return null;
            return AssemblyPackage.DeSerialise(p);
        }
        private void setPackageInfo(AssemblyPackage p)
        {
            byte[] data = p.Serialise();
            versionContainer.SetRootFileData(packageInfo, data);
        }

        public void AddVersion(SCMRevision assemblyRev, BuildServers.BuildArtifacts files)
        {
            PackageInfo = getPackageInfo(); // make sure we have actual version
            if (PackageInfo == null)
                PackageInfo = new AssemblyPackage();

            string revision = assemblyRev.Revision;

            AssemblyArtifacts pv = PackageInfo.AddRevision(files);

            assemblyRev.CreateAt = DateTime.Now;
            byte[] revdata = assemblyRev.Serialise();
            versionContainer.AddVersionData(revision + "/.revision", revdata);
            foreach (var artF in files.Artefacts)
            {
                versionContainer.AddVersionData(revision + "/" + artF.FileName, artF.Data);
                pv.AddArtefact(artF.FileName, artF);
            }
            setPackageInfo(PackageInfo);// write to container
        }

        public AssemblyVersionPackage GetLatestVersion()
        {
            Ref.AssemblyPackage pinfo = PackageInfo;
            if (pinfo == null)
            {
                Console.WriteLine("packageInfo absent in package {0}", this.Path);
                return null;
            }

            Ref.AssemblyArtifacts pv = pinfo.FindLatestVersion();
            AssemblyVersionPackage pckg = new AssemblyVersionPackage(pv, this);
            return pckg;
        }

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
        public Ref.AssemblyArtifacts LatestVersion
        {
            get
            {
                //return versionContainer.key_most_fresh;
                Ref.AssemblyPackage pinfo = PackageInfo;
                if (pinfo == null)
                    return null;
                Ref.AssemblyArtifacts pv = pinfo.FindLatestVersion();
                return pv;
            }
        }
        public SCMRevision LatestRevision
        {
            get
            {
                Ref.AssemblyPackage pinfo = PackageInfo;
                if (pinfo == null)
                    return null;
                Ref.AssemblyArtifacts pv = pinfo.FindLatestVersion();
                VersionData vd = versionContainer.GetSpecificVersionData(pv.VersionTag + "/.revision");
                SCMRevision vr = SCMRevision.DeSerialise(vd.data);
                return vr;
            }
        }
        public List<SCMRevision> GetVersions()
        {
            List<SCMRevision> vers = new List<SCMRevision>();
            foreach (string f in versionContainer.GetVersions())
            {
                VersionData vd = versionContainer.GetSpecificVersionData(f + "/.revision");
                if (vd == null)
                {
                    Console.WriteLine("binary version {0} unannotated with revision data in {1}", f, Path);
                    continue;
                }
                SCMRevision vr = SCMRevision.DeSerialise(vd.data);
                vr.CreateAt = vd.Created;
                vers.Add(vr);
            }
            return vers;
        }
    }
}
