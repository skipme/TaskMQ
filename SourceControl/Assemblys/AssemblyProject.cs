using SourceControl.Build;
using SourceControl.BuildServers;
using SourceControl.Containers;
using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TaskUniversum;

namespace SourceControl.Assemblys
{
    public class SourceController
    {
        public AssemblyBinVersions Versions;
        public string PackageName { get; private set; }
        public BuildServers.IBuildServer BuildServer;

        public bool RuntimeLoaded { get; set; }
        public string RuntimeLoadedRevision { get; set; }
        public string RuntimeLoadedRemark { get; set; }

        public List<SCMRevision> GetStoredVersions()
        {
            return Versions.GetVersions();
        }

        public SourceController(string workingDirectory, string moduleName, BuildServers.IBuildServer buildServer)
        {
            Versions = new AssemblyBinVersions(workingDirectory, moduleName);
            this.BuildServer = buildServer;

            this.PackageName = moduleName;
        }
        public void DoControl(SourceControllerJobs job)
        {
            switch (job)
            {
                case SourceControllerJobs.fetchBS:
                    if (BuildServer.DobBSJob(BuildServerJobs.fetch))
                    {
                        _BuildServerRevision = BuildServer.GetVersion();
                    }
                    break;
                case SourceControllerJobs.buildBS:
                    if (BuildServer.DobBSJob(BuildServerJobs.build))
                    {
                        _BuildServerRevision = BuildServer.GetVersion();
                    }
                    break;
                case SourceControllerJobs.updatePackageFromBuild:
                    SCMRevision buildVersion = BuildServer.GetVersion();
                    if (buildVersion != null && Versions.LatestVersion.VersionTag != buildVersion.Revision)
                    {
                        BuildArtifacts arts = BuildServer.GetArtifacts();
                        if (arts != null)// artifacts taken from build server
                        {
                            Versions.AddVersion(buildVersion, arts);
                            _PackageRevision = Versions.LatestRevision;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public List<SourceControllerJobs> GetAllowedJobs()
        {
            List<SourceControllerJobs> result = new List<SourceControllerJobs>();
            List<BuildServerJobs> bsjobs = BuildServer.GetAllowedJobs();

            HasAdd<BuildServerJobs, SourceControllerJobs>(BuildServerJobs.fetch, SourceControllerJobs.fetchBS,
                bsjobs, result);
            HasAdd<BuildServerJobs, SourceControllerJobs>(BuildServerJobs.build, SourceControllerJobs.buildBS,
               bsjobs, result);

            SCMRevision buildVersion = BuildServer.GetVersion();
            if (buildVersion != null && Versions.LatestVersion.VersionTag != buildVersion.Revision)
                result.Add(SourceControllerJobs.updatePackageFromBuild);

            return result;
        }
        private void HasAdd<T, D>(T item, D itemD, List<T> itemList, List<D> distList)
        {
            if (itemList.Contains(item))
                distList.Add(itemD);
        }

        SCMRevision _PackageRevision;
        DateTime LastCheck_PCKREV = DateTime.Now;
        public SCMRevision PackageRevision
        {
            get
            {
                if (_PackageRevision == null || (DateTime.Now - LastCheck_PCKREV).TotalSeconds > 80)
                {
                    _PackageRevision = Versions.LatestRevision;
                    LastCheck_PCKREV = DateTime.Now;
                }
                return _PackageRevision;
            }
        }

        SCMRevision _BuildServerRevision;
        DateTime LastCheck_BSREV = DateTime.Now;
        public SCMRevision BuildServerRevision
        {
            get
            {
                if (_BuildServerRevision == null || (DateTime.Now - LastCheck_BSREV).TotalSeconds > 20)
                {
                    _BuildServerRevision = BuildServer.GetVersion();
                    LastCheck_BSREV = DateTime.Now;
                }
                return _BuildServerRevision;
            }
        }

    }
}
