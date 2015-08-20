using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.BuildServers
{
    public class Register
    {
        public Dictionary<string, BuildServers.IBuildServer> BuildServersRegister;
        public Register()
        {
            BuildServersRegister = new Dictionary<string, BuildServers.IBuildServer>();

            SourceControl.BuildServers.TeamCityBS tc = new BuildServers.TeamCityBS();
            BuildServersRegister.Add(tc.Name, tc);

            if (TaskUniversum.Common.Runtime.IsRunningOnMono())
            {
                SourceControl.BuildServers.NaiveXBuildfromGit monogit = new BuildServers.NaiveXBuildfromGit();
                BuildServersRegister.Add(monogit.Name, monogit);
            }
            else
            {
                SourceControl.BuildServers.NaiveMSfromGit netgit = new BuildServers.NaiveMSfromGit();
                BuildServersRegister.Add(netgit.Name, netgit);
            }
        }

      

        public TaskQueue.Providers.TItemModel GetParametersModel(string buildServerType)
        {
            return BuildServersRegister[buildServerType].GetParametersModel();
        }
        public BuildServers.IBuildServer GetNewInstance(string buildServerType)
        {
            BuildServers.IBuildServer dicbs;
            if (BuildServersRegister.TryGetValue(buildServerType, out dicbs))
            {
                Type t = dicbs.GetType();
                object obj = Activator.CreateInstance(t);
                return obj as BuildServers.IBuildServer;
            }
            return null;
        }
    }
}
