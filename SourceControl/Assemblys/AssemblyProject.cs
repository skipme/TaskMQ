using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.Assemblys
{
    public class AssemblyProject
    {
        public string ProjectPath { get; private set; }
        public string SCM_uri { get; private set; }

        private AssemblySource Source;
        private AssemblyBinVersions Versions;

        public List<VersionRevision> GetStoredVersions()
        {
            return Versions.GetVersions();
        }
        public AssemblyProject(string rootDirectory, string relativeProjectPath, string remoteUri)
        {
            // git only
            Source = new AssemblySource(rootDirectory, relativeProjectPath, remoteUri);
            Versions = new AssemblyBinVersions(rootDirectory, Source.Name);

            ProjectPath = Source.ProjectFilePath;
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
        public bool GetSpecificVersion(string revision, out byte[] library, out byte[] symbols)
        {
            if (GetStoredVersions().Where(v => v.Revision == revision).Count() == 1)
            {
                return Versions.GetSpecificVersion(revision, out library, out symbols);
            }
            else
            {
                library = symbols = null;
                return false;
            }
        }
        public void StoreNewIfRequired()
        {
            VersionRevision rev = sourceVersionRevision;
            if (rev == null)
                return;
            
            if (rev.Revision != edgeStoredVersionRevision)
            {
                if (Source.SetUpToDate())
                {
                    byte[] lib, sym;
                    if (Source.BuildProject(out lib, out sym))
                    {
                        Versions.AddVersion(rev, lib, sym);
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
