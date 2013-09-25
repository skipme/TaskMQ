using SourceControl.Build;
using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SourceControl.Assemblys
{
    public class AssemblySource
    {
        public string SourceUriOrigin { get; private set; }// repository url
        public string ProjectFilePath { get; private set; }
        public string Name { get; set; }
        private bool IsUpToDate { get; set; }
        public string lastBuildLog { get; private set; }

        private SCM scm;
        public SCM.Status Status
        {
            get
            {
                return scm.CurrentStatus;
            }
        }

        public VersionRevision Version
        {
            get
            {
                if (scm.LocalVersion == null && scm.CurrentStatus == SCM.Status.none)
                    scm.UpdateStatus();
                return scm.LocalVersion;
            }
        }

        public AssemblySource(string root, string projFileRelativePath, string remoteUri)
        {
            SourceUriOrigin = remoteUri;
            //Name = System.IO.Path.GetFileNameWithoutExtension(projFileRelativePath);
            Uri u = new Uri(remoteUri);
            Name = System.IO.Path.GetFileNameWithoutExtension(u.AbsolutePath);
            ProjectFilePath = System.IO.Path.Combine(root, Name, projFileRelativePath);
            string dir = System.IO.Path.Combine(root, Name);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            Type T = typeof(Git.git_scm);
            scm = (SCM)Activator.CreateInstance(T, new object[] { dir, remoteUri });
        }

        //public bool BuildProject(out string outputLocation, out byte[] library, out byte[] symbols, out string[] assetsPath)
        public bool BuildProject(out AssemblyBinaryBuildResult bin)
        {
            bool bresult;
            bin = null;

            AssemblyBuilder builder = new AssemblyBuilder(ProjectFilePath);
            if (bresult = builder.BuildProject())
            {
                bin = AssemblyBinaryBuildResult.FromFile(builder.BuildResultDll, builder.BuildResultSymbols);
                bin.assetsRoot = System.IO.Path.GetDirectoryName(builder.BuildResultDll);
                bin.assets = builder.BuildResultAssets;
            }
            else
            {
                Console.WriteLine("build project failure: {0}", builder.Log);
            }
            lastBuildLog = builder.Log;

            return bresult;
        }
        public bool SetUpToDate()
        {
            return IsUpToDate = lSetUpToDate();
        }
        private bool lSetUpToDate()
        {
            switch (scm.CurrentStatus)
            {
                case SCM.Status.none:
                    scm.UpdateStatus();
                    if (scm.CurrentStatus != SCM.Status.none)
                        return lSetUpToDate();
                    return false;
                // 
                case SCM.Status.cloneFailure:
                    return scm.Clone();
                case SCM.Status.cloneRequired:
                    return scm.Clone();
                //
                case SCM.Status.fetchFailure:
                    return scm.Fetch();
                case SCM.Status.fetchRequied:
                    return scm.Fetch();

                case SCM.Status.allUpToDate:
                    return true;
                default:
                    break;
            }
            return false;
        }
    }
}
