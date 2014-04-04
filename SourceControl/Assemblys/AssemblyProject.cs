using SourceControl.Build;
using SourceControl.BuildServers;
using SourceControl.Containers;
using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SourceControl.Assemblys
{
    public class AssemblyProject
    {
        public bool BuildDeferred { get; private set; }
        public bool FetchDeferred { get; private set; }
        public bool UpdateDeferred { get; private set; }

        public void SetBuildDeferredFlag()
        {
            BuildDeferred = true;
        }
        public void SetFetchDeferredFlag()
        {
            FetchDeferred = true;
        }
        public void SetUpdateDeferredFlag()
        {
            UpdateDeferred = true;
        }

        public AssemblyBinVersions Versions;
        public string moduleName { get; private set; }
        public BuildServers.IBuildServer BuildServer;

        public bool RuntimeLoaded { get; set; }
        public string RuntimeLoadedRevision { get; set; }
        public string RuntimeLoadedRemark { get; set; }

        public List<SCMRevision> GetStoredVersions()
        {
            return Versions.GetVersions();
        }

        public AssemblyProject(string workingDirectory, string moduleName, BuildServers.IBuildServer buildServer)
        {
            Versions = new AssemblyBinVersions(workingDirectory, moduleName);
            this.BuildServer = buildServer;

            this.moduleName = moduleName;
        }
        public void Fetch()
        {
            BuildServer.FetchSource();
            _BuildServerRevision = BuildServer.GetVersion();
        }
        public void Build()
        {
            SCMRevision buildVersion = BuildServer.GetVersion();
            if (Versions.LatestVersion.VersionTag != buildVersion.Revision)
            {
                BuildServer.BuildSource();
                _BuildServerRevision = BuildServer.GetVersion();
            }
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

        public void UpdatePackage()
        {
            SCMRevision buildVersion = BuildServer.GetVersion();
            if (Versions.LatestVersion.VersionTag != buildVersion.Revision)
            {
                BuildArtifacts arts = BuildServer.GetArtifacts();
                if (arts == null)
                    return;
                Versions.AddVersion(buildVersion, arts);
                _PackageRevision = Versions.LatestRevision;
            }
        }
    }
}
