using FileContentArchive;
using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TaskUniversum;

namespace SourceControl.Containers
{
    /// <summary>
    /// Assembly package container
    /// </summary>
    public class AssemblyPackage
    {
        const string packageInfo = "package.json";
        public ContentVersionStorage versionContainer;
        public string Path { get; private set; }
        public string name { get; private set; }

        static ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        public DateTime LastPackagedDate
        {
            get
            {
                return GetLatestVersion().Version.AddedAt;
            }
        }
        public PackageInfo PackageInfo { get; private set; }

        public AssemblyPackage(string directoryScope, string name)
        {
            this.Path = System.IO.Path.Combine(directoryScope, name + ".zip");
            this.name = name;

            if (!File.Exists(Path))
                logger.Error("Assembly package not found, will be recreated.");

            versionContainer = new FileContentArchive.ContentVersionStorage(new FileContentArchive.ZipStorage(Path));

            PackageInfo = getPackageInfo();
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

        public void AddVersion(SCMRevision assemblyRev, BuildServers.BuildArtifacts files)
        {
            PackageInfo = getPackageInfo(); // make sure we have actual version
            if (PackageInfo == null)
                PackageInfo = new PackageInfo();

            string revision = assemblyRev.Revision;

            PackageInfoArtifacts declaredRevision = PackageInfo.NewRevision(files);

            assemblyRev.CreateAt = DateTime.Now;
            byte[] revdata = assemblyRev.Serialise();
            versionContainer.AddVersionData(revision + "/.revision", revdata);
            foreach (var artF in files.Artefacts)
            {
                versionContainer.AddVersionData(revision + "/" + artF.FileName, artF.Data);
                declaredRevision.AddArtefact(artF.FileName, artF);
            }
            setPackageInfo(PackageInfo);// write to container
        }

        public AssemblyPackageVersionHelper GetLatestVersion()
        {
            Ref.PackageInfo pinfo = PackageInfo;
            if (pinfo == null)
            {
                //Console.WriteLine("packageInfo absent in package {0}", this.Path);
                logger.Error("packageInfo absent in package {0}", this.Path);
                return null;
            }

            Ref.PackageInfoArtifacts pv = pinfo.FindLatestVersion();
            AssemblyPackageVersionHelper pckg = new AssemblyPackageVersionHelper(pv, this);
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
        public Ref.PackageInfoArtifacts LatestVersion
        {
            get
            {
                //return versionContainer.key_most_fresh;
                Ref.PackageInfo pinfo = PackageInfo;
                if (pinfo == null)
                    return null;
                Ref.PackageInfoArtifacts pv = pinfo.FindLatestVersion();
                return pv;
            }
        }
        public SCMRevision LatestRevision
        {
            get
            {
                Ref.PackageInfo pinfo = PackageInfo;
                if (pinfo == null)
                    return null;
                Ref.PackageInfoArtifacts pv = pinfo.FindLatestVersion();
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
