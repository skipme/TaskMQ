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
       
        SourceControl.Assemblys.AssemblySCM<Build.AssemblyBuilderMono> _scm;
        SourceControl.Assemblys.AssemblySCM<Build.AssemblyBuilderMono> scm
        {
            get
            {
                if (_scm == null)
                {
                    _scm = new Assemblys.AssemblySCM<Build.AssemblyBuilderMono>(
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

        public void FetchSource()
        {
            if (scm.Status == SCM.Status.allUpToDate)
            {
                State = BuildServerState.fetch_ok;
                return;
            }

            State = BuildServerState.fetch;

            bool result = scm.SetUpToDate();
            logger.Debug("gitmono bs: source '{0}' update: {1}", scm.Name, result ? "ok" : "fail");
            State = result ? BuildServerState.fetch_ok : BuildServerState.fetch_error;
        }

        public void BuildSource()
        {
            //if (scm.Status != SCM.Status.allUpToDate)
            //    return;
            State = BuildServerState.build;
            bool result = scm.BuildProject(parameters.Configuration);
            State = result ? BuildServerState.build_ok : BuildServerState.build_error;
        }
    }

}
