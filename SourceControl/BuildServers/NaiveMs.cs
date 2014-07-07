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
    public class NaiveMSfromGit : IBuildServer
    {
        ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        NaiveMSfromGitParams parameters = new NaiveMSfromGitParams();

        public NaiveMSfromGit()
        {
            State = BuildServerState.fetch_required;
        }

        SourceControl.Assemblys.AssemblySCM<Build.AssemblyBuilder> _scm;
        SourceControl.Assemblys.AssemblySCM<Build.AssemblyBuilder> scm
        {
            get
            {
                if (_scm == null)
                {
                    _scm = new Assemblys.AssemblySCM<Build.AssemblyBuilder>(
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
            get { return "naive .NET msbuild and git scm"; }
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
            logger.Debug("gitms bs: source '{0}' update: {1}", scm.Name, result ? "ok" : "fail");
            State = result ? BuildServerState.fetch_ok : BuildServerState.fetch_error;
        }

        public void BuildSource()
        {
            //if (scm.Status != SCM.Status.allUpToDate)
            //    return;
            State = BuildServerState.build;
            bool result = scm.BuildProject();
            State = result ? BuildServerState.build_ok : BuildServerState.build_error;
        }
    }
    public class NaiveMSfromGitParams : TItemModel
    {
        public NaiveMSfromGitParams() { }
        public NaiveMSfromGitParams(TItemModel tm) : base(tm.GetHolder()) { }

        [TaskQueue.FieldDescription("project in source for build", Required: true)]
        public string ProjectPath { get; set; }

        [TaskQueue.FieldDescription("built assembly relative path", Required: true)]
        public string AssemblyPath { get; set; }

        [TaskQueue.FieldDescription("scm url", Required: true)]
        public string SCM_URL { get; set; }

        //[TaskQueue.FieldDescription("scm files location", Required: true)]
        //public string WorkingDirectory { get; set; }

        //[TaskQueue.FieldDescription("username", Required: false)]
        //public string User { get; set; }
        //[TaskQueue.FieldDescription("password", Required: false)]
        //public string Password { get; set; }

        [FieldDescription(Ignore = true, Inherited = true, Required = false)]
        public override string ItemTypeName
        {
            get
            {
                return "naive.Net.Git parameters";
            }
            set
            {

            }
        }

        public bool ValidateValues(out string result)
        {
            result = string.Empty;
            if (string.IsNullOrWhiteSpace(ProjectPath))
            {
                result += ";\n" + "parameter " + "ProjectPath" + " is empty";
            }
            if (string.IsNullOrWhiteSpace(AssemblyPath))
            {
                result += ";\n" + "parameter " + "AssemblyPath" + " is empty";
            }
            if (string.IsNullOrWhiteSpace(SCM_URL))
            {
                result += ";\n" + "parameter " + "SCM_URL" + " is empty";
            }

            return result == string.Empty;
        }
    }
}
