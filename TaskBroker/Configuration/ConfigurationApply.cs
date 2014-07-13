using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskBroker.Configuration;
using TaskUniversum;

namespace TaskBroker
{
    public static class ConfigurationApply
    {
        static ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();
        public static void Apply(this ConfigurationBroker con, Broker broker)
        {
            logger.Debug("trying to apply main configuration with datetime stamp: {0}", con.CreationDate);

            foreach (var connection in con.Connections)
            {
                var qinterface = broker.QueueInterfaces.GetQueue(connection.queueTypeName);
                broker.RegisterConnection(connection.Name, connection.connectionString,
                    connection.database, connection.collection, qinterface);
            }
            foreach (var channel in con.Channels)
            {
                broker.RegisterChannel(channel.connectionName, channel.Name);
            }
            for (int i = 0; i < con.Tasks.Length; i++)
            {
                var task = con.Tasks[i];
                if (task == null)
                {
                    logger.Warning("skipping task from configuration at index: {0}", i);
                }
                else
                    if (!task.Auto)
                    {
                        broker.RegisterTask(task.ChannelName, task.ModuleName, task.intervalType, task.intervalValue, task.parameters, task.Description);
                    }
            }
        }
        public static void Apply(this ConfigurationAssemblys con, Broker broker)
        {
            logger.Debug("trying to apply assembly's configuration with datetime stamp: {0}", con.CreationDate);

            foreach (var assemblyProject in con.Assemblys)
            {
                broker.AddAssembly(assemblyProject.Name, assemblyProject.BuildServerType, assemblyProject.BSParameters);
            }

        }
    }
}
