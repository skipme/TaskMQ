using SourceControl.Build;
using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TaskUniversum;

namespace SourceControl.BuildServers
{
    public class AssemblySCM<BS> where BS : IAssemblyBuilder
    {
        ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        public string SourceUriOrigin { get; private set; }
        // repository url
        public string ProjectFilePath { get; private set; }

        public string Name { get; set; }

        private bool IsUpToDate { get; set; }

        public string lastBuildLog { get; private set; }

        public string WorkingDir { get; set; }

        private SCM scm;

        public SCM.Status Status
        {
            get
            {
                return scm.CurrentStatus;
            }
        }

        public SCMRevision Version
        {
            get
            {
                if (scm.LocalVersion == null && scm.CurrentStatus == SCM.Status.none)
                    scm.UpdateStatus();
                return scm.LocalVersion;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workingDir">dir where to save all source and package</param>
        /// <param name="projFileRelativePath">project file</param>
        /// <param name="remoteUri">scm URL </param>
        public AssemblySCM(string workingDir, string projFileRelativePath, string remoteUri)
        {
            SourceUriOrigin = remoteUri;

            Uri u = new Uri(remoteUri);
            Name = System.IO.Path.GetFileNameWithoutExtension(u.AbsolutePath);

            this.ProjectFilePath = System.IO.Path.Combine(workingDir, Name, projFileRelativePath);
            this.WorkingDir = System.IO.Path.Combine(workingDir, Name);
            if (!System.IO.Directory.Exists(this.WorkingDir))
            {
                System.IO.Directory.CreateDirectory(this.WorkingDir);
            }

            Type T = typeof(Git.git_scm);
            scm = (SCM)Activator.CreateInstance(T, new object[] { this.WorkingDir, remoteUri });
        }

        public bool BuildProject(string Configuration, out AssemblyBinaryBuildResult bin)
        {
            bool bresult;
            bin = null;

            IAssemblyBuilder builder = (IAssemblyBuilder)Activator.CreateInstance(typeof(BS), new object[] { ProjectFilePath, Configuration });

            if (bresult = builder.BuildProject())
            {
                bin = AssemblyBinaryBuildResult.FromFile(builder.BuildResultDll, builder.BuildResultSymbols, builder.BuildResultAssets);
            }
            else
            {
                logger.Error("build project failure: {0}", builder.Log);
            }
            lastBuildLog = builder.Log;

            return bresult;
        }

        public bool BuildProject(string Configuration)
        {
            bool bresult;

            IAssemblyBuilder builder = (IAssemblyBuilder)Activator.CreateInstance(typeof(BS), new object[] { ProjectFilePath, ProjectFilePath });
            if (!(bresult = builder.BuildProject()))
            {
                logger.Error("build project failure: {0}", builder.Log);
            }
            lastBuildLog = builder.Log;

            return bresult;
        }

        public BuildServers.BuildArtifacts GetArtifacts(string assemblyLocation)
        {
            string assemblyAbs = System.IO.Path.Combine(this.WorkingDir, assemblyLocation);
            if (!File.Exists(assemblyAbs))
            {
                logger.Error("GetArtifacts can't find output assembly");
                return null;
            }

            return BuildServers.BuildArtifacts.FromDirectory(assemblyAbs, Version.Revision);
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
