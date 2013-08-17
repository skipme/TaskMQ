using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.Assemblys
{
    public class AssemblyProject
    {
        public string LocalPath { get; private set; }
        public string SCM_uri { get; private set; }

        private AssemblySource Source;
        private AssemblyBinVersions Versions;

        public AssemblyProject(string rootDirectory, string relativeProjectPath, string remoteUri)
        {
            // git only
            Source = new AssemblySource(rootDirectory, relativeProjectPath, remoteUri);
            Versions = new AssemblyBinVersions(rootDirectory, Source.Name);

            LocalPath = Source.ProjectFilePath;
            SCM_uri = remoteUri;
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
        public void StoreNewIfRequired()
        {
            if (sourceVersionRevision == null)
                return;
            if (sourceVersionRevision != storedVersionRevision)
            {
                if (Source.SetUpToDate())
                {
                    byte[] lib, sym;
                    if (Source.BuildProject(out lib, out sym))
                    {
                        Versions.AddVersion(Source.Version, lib, sym);
                    }
                }
            }
        }
        public string sourceVersionRevision
        {
            get
            {
                return Source.Version;
            }
        }
        public string storedVersionRevision
        {
            get
            {
                return Versions.LatestRevision;
            }
        }
    }
}
