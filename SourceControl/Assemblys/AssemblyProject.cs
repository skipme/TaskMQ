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
        public bool BuildDeferred { get; set; }
        public void SetBuildDeferredFlag()
        {
            BuildDeferred = true;
        }

        public AssemblyBinVersions Versions;
        public string moduleName { get; private set; }
        public BuildServers.IBuildServer BuildServer;

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
        }
        public void Build()
        {

        }
        public void UpdatePackage()
        {
            SCMRevision buildVersion = BuildServer.GetVersion();
            if (Versions.LatestRevision.VersionTag != buildVersion.Revision)
            {
                BuildArtifacts arts = BuildServer.GetArtifacts();
                Versions.AddVersion(buildVersion, arts);
            }
        }
    }
}
