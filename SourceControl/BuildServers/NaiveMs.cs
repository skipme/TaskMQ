using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;
using SourceControl.BuildServers.TeamCity;

namespace SourceControl.BuildServers
{
    public class NaiveMSfromGit : BuildServer
    {

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
            throw new NotImplementedException();
        }

        public void SetParameters(TItemModel parameters)
        {
            throw new NotImplementedException();
        }

        public BuildArtifacts GetArtifactsZip()
        {
            throw new NotImplementedException();
        }

        public bool CheckParameters(out string explanation)
        {
            throw new NotImplementedException();
        }

        public SCMRevision GetVersion()
        {
            throw new NotImplementedException();
        }
    }
    public class NaiveMSfromGitParams : TItemModel
    {
        public NaiveMSfromGitParams() { }
        public NaiveMSfromGitParams(TItemModel tm) : base(tm.GetHolder()) { }

        [TaskQueue.FieldDescription("project in source for build", Required: true)]
        public string ProjectPath { get; set; }

        [TaskQueue.FieldDescription("scm url", Required: true)]
        public string Host { get; set; }

        //[TaskQueue.FieldDescription("username", Required: false)]
        //public string User { get; set; }
        //[TaskQueue.FieldDescription("password", Required: false)]
        //public string Password { get; set; }

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
    }
}
