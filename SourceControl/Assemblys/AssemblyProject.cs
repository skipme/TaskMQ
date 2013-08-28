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
        public bool GetSpecificVersion(string revision, out AssemblyBinary binary)
        {
            if (GetStoredVersions().Where(v => v.Revision == revision).Count() == 1)
            {
                return Versions.GetSpecificVersion(revision, out binary);
            }
            else
            {
                binary = null;
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
                    string[] assets = null;
                    string reldir = null;
                    AssemblyBinary bin;
                    if (Source.BuildProject(out bin, out assets, out reldir))
                    {
                        if (reldir.EndsWith("/") || reldir.EndsWith("\\"))
                            reldir = reldir.Remove(reldir.Length - 1);
                        var iassets = from a in assets
                                      select new BuildResultFile
                                      {
                                          Data = File.ReadAllBytes(a),
                                          Name = a.Replace(reldir, "").Remove(0, 1)
                                      };
                        Versions.AddVersion(rev, bin.library, bin.symbols, iassets);
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
