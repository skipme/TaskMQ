using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue;
using TaskQueue.Providers;

namespace SourceControl.BuildServers
{
    public enum BuildServerState
    {
        source_absent,
        fetch,
        build,
        fetch_required,
        fetch_error,
        build_error,
        fetch_ok,
        build_ok,
        major_error
    }
    public enum BuildServerJobs
    {
        fetch,
        build
    }
    public interface IBuildServer
    {
        string Name { get; }
        string Description { get; }

        TItemModel GetParametersModel();
        void SetParameters(Dictionary<string, object> Mparameters);

        BuildArtifacts GetArtifacts();
        bool CheckParameters(out string explanation);

        SCMRevision GetVersion();
        BuildServerState GetState();

        bool DobBSJob(BuildServerJobs jobDef);
        List<BuildServerJobs> GetAllowedJobs();
        //void FetchSource();
        //void BuildSource();
    }
}
