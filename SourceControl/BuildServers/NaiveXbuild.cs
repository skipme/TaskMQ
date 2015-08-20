using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;
using SourceControl.BuildServers.TeamCity;
using TaskQueue;
using TaskUniversum;

namespace SourceControl.BuildServers
{
    public class NaiveXBuildfromGit : IBuildServer
    {
        ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        NaiveMSfromGitParams parameters = new NaiveMSfromGitParams();

        public NaiveXBuildfromGit()
        {
            State = BuildServerState.fetch_required;
        }

        SourceControl.BuildServers.AssemblySCM<Build.AssemblyBuilderMono> _scm;
        SourceControl.BuildServers.AssemblySCM<Build.AssemblyBuilderMono> scm
        {
            get
            {
                if (_scm == null)
                {
                    _scm = new BuildServers.AssemblySCM<Build.AssemblyBuilderMono>(
                        System.IO.Directory.GetCurrentDirectory(),
                        parameters.ProjectPath,
                        parameters.SCM_URL
                        );
                }
                return _scm;
            }
        }
        BuildServerState State;

        public string Name
        {
            get { return "naive.NET.GIT"; }
        }

        public string Description
        {
            get { return "naive MONO Xbuid and git scm"; }
        }

        public TItemModel GetParametersModel()
        {
            return parameters;
        }

        public void SetParameters(Dictionary<string, object> Mparameters)
        {
            this.parameters.SetHolder(Mparameters);
        }
        public void SetParameters(NaiveMSfromGitParams parameters)
        {
            this.parameters = parameters;
        }
        public BuildArtifacts GetArtifacts()
        {
            return scm.GetArtifacts(parameters.AssemblyPath);
        }

        public bool CheckParameters(out string explanation)
        {
            if (!parameters.ValidateValues(out explanation))
                return false;
            explanation = string.Empty;
            return true;
        }

        public SCMRevision GetVersion()
        {
            return scm.Version;
        }

        public BuildServerState GetState()
        {
            return State;
        }

        private bool FetchSource()
        {
            if (scm.Status == SCM.Status.allUpToDate)
            {
                State = BuildServerState.fetch_ok;
                return true;
            }

            State = BuildServerState.fetch;

            bool result = scm.SetUpToDate();
            logger.Debug("gitmono bs: source '{0}' update: {1}", scm.Name, result ? "ok" : "fail");
            State = result ? BuildServerState.fetch_ok : BuildServerState.fetch_error;

            return result;
        }

        private bool BuildSource()
        {
            State = BuildServerState.build;
            bool result = scm.BuildProject(parameters.Configuration);
            State = result ? BuildServerState.build_ok : BuildServerState.build_error;

            return result;
        }

        public bool DobBSJob(BuildServerJobs jobDef)
        {
            bool result = false;
            switch (jobDef)
            {
                case BuildServerJobs.fetch:
                    result = FetchSource();
                    break;
                case BuildServerJobs.build:
                    result = BuildSource();
                    break;
                default:
                    break;
            }

            return result;
        }

        public List<BuildServerJobs> GetAllowedJobs()
        {
            List<BuildServerJobs> result = new List<BuildServerJobs>();
            switch (State)
            {
                case BuildServerState.source_absent:
                    result.Add(BuildServerJobs.fetch);
                    break;
                case BuildServerState.fetch:
                    break;
                case BuildServerState.build:
                    break;
                case BuildServerState.fetch_required:
                    result.Add(BuildServerJobs.fetch);
                    break;
                case BuildServerState.fetch_error:
                    result.Add(BuildServerJobs.fetch);
                    break;
                case BuildServerState.build_error:
                    result.Add(BuildServerJobs.fetch);
                    result.Add(BuildServerJobs.build);
                    break;
                case BuildServerState.fetch_ok:
                    result.Add(BuildServerJobs.build);
                    break;
                case BuildServerState.build_ok:
                    break;
                case BuildServerState.major_error:
                    result.Add(BuildServerJobs.fetch);
                    result.Add(BuildServerJobs.build);
                    break;
                default:
                    break;
            }

            return result;
        }
    }

}
