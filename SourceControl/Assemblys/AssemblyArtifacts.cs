using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.Assemblys
{
    public class AssemblyArtifacts
    {
        public Dictionary<string, BuildServers.IBuildServer> BuildServers;
        public AssemblyArtifacts()
        {
            BuildServers = new Dictionary<string, BuildServers.IBuildServer>();
            SourceControl.BuildServers.TeamCityBS tc = new BuildServers.TeamCityBS();
            SourceControl.BuildServers.NaiveMSfromGit netgit = new BuildServers.NaiveMSfromGit();

            BuildServers.Add(tc.Name, tc);
            BuildServers.Add(netgit.Name, netgit);
        }

      

        public TaskQueue.Providers.TItemModel GetParametersModel(string buildServerType)
        {
            return BuildServers[buildServerType].GetParametersModel();
        }
        public BuildServers.IBuildServer GetNewInstance(string buildServerType)
        {
            Type t = BuildServers[buildServerType].GetType();
            object obj = Activator.CreateInstance(t);
            return obj as BuildServers.IBuildServer;
        }
    }
}
