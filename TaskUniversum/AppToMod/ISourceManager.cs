using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskUniversum.Common;

namespace TaskUniversum
{
    public enum SourceControllerJobs
    {
        fetchBS,
        buildBS,
        updatePackageFromBuild
    }

    public interface ISourceManager
    {
        //void DoPackageCommand(string Name, SourceControllerJobs job);
        List<SourceControllerJobs> GetAllowedCommands(string Name);

        bool CheckBuildServerParameters(string BSTypeName, Dictionary<string, object> bsParameters, out string explain);
        IRepresentedConfiguration GetJsonBuildServersConfiguration();
    }
}
