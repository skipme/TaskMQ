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
            RepresentedConfiguration c = new RepresentedConfiguration();
            c.Connections = (from cc in b.MessageChannels.Connections
                             select new cConnection()
                             {
                                 Name = cc.Key,
                                 connectionString = cc.Value.ConnectionString,
                                 collection = cc.Value.Collection,
                                 database = cc.Value.Database,
                                 queueTypeName = cc.Value.QueueTypeName
                             }).ToArray();
            c.Channels = (from cc in b.MessageChannels.MChannelsList
                          select new cChannel()
                          {
                              connectionName = cc.ConnectionName,
                              Name = cc.UniqueName,
                              messageModel = new cModel() {  TypeFullName = cc.MessageModel.GetType().FullName },
                          }).ToArray();
            c.Modules = (from mm in b.Modules.Modules
                         select new cModule()
                         {
                             TypeFullName = mm.Value.MI.GetType().FullName
                         }).ToArray();
            c.Tasks = (from tt in b.Tasks
                       select new cTask()
                       {
                           intervalType = tt.intervalType,
                           Description = tt.Description,
                           ChannelName = tt.ChannelName,
                           intervalValue = tt.intervalValue,
                           ModuleName = tt.Module.UniqueName,
                           parameters = tt.Parameters.ToDictionary()
                       }).ToArray();

            return c;
        }
        public static void ConfigureBroker(Broker b, RepresentedConfiguration c)
        {

        }
    }
}
