using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue;

namespace TaskBroker.Configuration
{
    public class BrokerConfiguration
    {
        public static RepresentedConfiguration ExtractFromBroker(Broker b)
        {
            ConfigurationBroker c = new ConfigurationBroker();
            c.Connections = (from cc in b.MessageChannels.Connections
                             select new cConnection()
                             {
                                 Name = cc.Key,
                                 queueTypeName = cc.Value.QueueTypeName,
                                 QueueParameters = cc.Value.specParams.GetHolder(),
                                 Auto = cc.Value.Temporary
                             }).ToArray();
            c.Channels = (from cc in b.MessageChannels.MChannelsList
                          select new cChannel()
                          {
                              connectionName = cc.ConnectionName,
                              Name = cc.UniqueName,
                              Auto = cc.Temporary
                          }).ToArray();

            c.Tasks = (from tt in b.Tasks
                       select new cTask()
                       {
                           intervalType = tt.intervalType,
                           Description = tt.NameAndDescription,
                           ChannelName = tt.ChannelName,
                           intervalValue = tt.intervalValue,
                           ModuleName = tt.Module.UniqueName,
                           parameters = tt.Parameters == null ? null : tt.Parameters,
                           Auto = tt.Temporary
                       }).ToArray();

            return c;
        }
        public static RepresentedConfiguration ExtractModulesFromBroker(Broker b)
        {
            ConfigurationModules c = new ConfigurationModules();

            c.Modules = (from mm in b.Modules.Modules
                         select new cModule()
                         {
                             TypeFullName = mm.Value.MI.GetType().FullName,
                             Name = mm.Key,
                             Description = mm.Value.Description,
                             Role = mm.Value.Role,
                             ParametersModel = mm.Value.ParametersModel.schema.ToList().ToDictionary((keyItem) => keyItem.Value1, (valueItem) => new Configuration.SchemeValueSpec(valueItem.Value2))
                             //mm.Value.ParametersModel.schema
                         }).ToArray();

            return c;
        }
        public static RepresentedConfiguration ExtractAssemblysFromBroker(Broker b)
        {
            ConfigurationAssemblys c = new ConfigurationAssemblys();

            c.Assemblys = (from mm in b.AssemblyHolder.assemblySources.hostedProjects
                           select new cAssembly()
                           {
                               Name = mm.PackageName,
                               BSParameters = mm.BuildServer.GetParametersModel().GetHolder(),
                               BuildServerType = mm.BuildServer.Name
                           }).ToArray();

            return c;
        }
        public static void ConfigureBroker(Broker b, RepresentedConfiguration c)
        {

        }
    }
}
