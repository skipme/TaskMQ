using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public interface BuildServer
    {
        string Name { get; }
        string Description { get; }

        TItemModel GetParametersModel();
        void SetParameters(TItemModel Mparameters);

        BuildArtifacts GetArtifacts();
        bool CheckParameters(out string explanation);

        SCMRevision GetVersion();
        BuildServerState GetState();

        void FetchSource();
        void BuildSource();
    }
}
