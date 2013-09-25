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
        //public AssemblyProject(string rootDirectory, string relativeProjectPath, string remoteUri)
        //{
        // git only
        //Source = new AssemblySource(rootDirectory, relativeProjectPath, remoteUri);+
        public AssemblyProject(string rootDirectory, AssemblySource source)
        {
            string Name = System.IO.Path.GetFileNameWithoutExtension(source.ProjectFilePath);
            Versions = new AssemblyBinVersions(rootDirectory, Name);
            this.Source = source;
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
        public void StoreNewIfRequired()
        {
            VersionRevision rev = sourceVersionRevision;
            if (rev == null)
                return;

            if (rev.Revision != edgeStoredVersionRevision)
            {
                if (Source.SetUpToDate())
                {
                    AssemblyBinaryBuildResult bin;
                    if (Source.BuildProject(out bin))
                    {
                        Versions.AddVersion(rev, bin);
                    }
                }
            }
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
