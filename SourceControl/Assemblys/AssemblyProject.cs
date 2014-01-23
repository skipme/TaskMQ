using SourceControl.Build;
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

        private AssemblyBinVersions Versions;
        public string moduleName { get; private set; }

        public List<SCMRevision> GetStoredVersions()
        {
            return Versions.GetVersions();
        }

        public AssemblyProject(string workingDirectory, string moduleName)
        {
            Versions = new AssemblyBinVersions(workingDirectory, moduleName);
        }
    }
}
