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
        private AssemblySource Source;
        private AssemblyBinVersions Versions;

        public List<VersionRevision> GetStoredVersions()
        {
            return Versions.GetVersions();
        }

        public AssemblyProject(string workingDirectory, string projectRelativePath, string scmUrl, string moduleName)
        {
            this.Source = new SourceControl.Assemblys.AssemblySource(workingDirectory, projectRelativePath, scmUrl);
            Versions = new AssemblyBinVersions(workingDirectory, moduleName);
        }
        public bool IsSourceUpToDate
        {
            get
            {
                return Source.Status == SCM.Status.allUpToDate;
            }
        }
        public bool SetUpSourceToDate()
        {
            bool result = Source.SetUpToDate();
            Console.WriteLine("source '{0}' update: {1}", Source.Name, result ? "ok" : "fail");
            return result;
        }
        //public bool GetSpecificVersion(string revision, out AssemblyBinaryBuildResult binary)
        //{
        //    if (GetStoredVersions().Where(v => v.Revision == revision).Count() == 1)
        //    {
        //        return Versions.GetSpecificVersion(revision, out binary);
        //    }
        //    else
        //    {
        //        binary = null;
        //        return false;
        //    }
        //}
        public bool StoreNewIfRequired(out string buildLog)
        {
            buildLog = string.Empty;
            VersionRevision rev = sourceVersionRevision;
            if (rev == null)
                return false;

            bool result = false;
            if (rev.Revision != edgeStoredVersionRevision)
            {
                if (Source.SetUpToDate())
                {
                    AssemblyBinaryBuildResult bin;
                    if (result = Source.BuildProject(out bin))
                    {
                        Versions.AddVersion(rev, bin);
                        result = true;
                    }
                }                
            }
            buildLog = Source.lastBuildLog;
            return result;
        }
        public VersionRevision sourceVersionRevision
        {
            get
            {
                return Source.Version;
            }
        }
        public string edgeStoredVersionRevision
        {
            get
            {
                return Versions.LatestRevision;
            }
        }
    }
}
