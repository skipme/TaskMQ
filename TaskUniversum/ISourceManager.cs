using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum
{
    public interface ISourceManager
    {
        void BuildSource(string assemblyProjectName);
        void FetchSource(string assemblyProjectName);
        void UpdatePackage(string assemblyProjectName);

        bool CheckBuildServerParameters(string BSTypeName, Dictionary<string, object> bsParameters, out string explain);
        string GetJsonBuildServersConfiguration();
    }
}
