using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskBroker.Configuration;
using TaskQueue.Providers;
using TaskUniversum;

namespace TaskBroker
{
    public static class ConfigurationApply
    {
        static ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();
        public static void Apply(this ConfigurationBroker con, Broker broker)
        {
            logger.Debug("Trying to apply ---main configuration---\r\n with datetime stamp: {0} {1}", con.CreationDate.ToLongDateString(), con.CreationDate.ToLongTimeString());

            for (int i = 0; i < con.Connections.Length; i++)
            {
                cConnection connection = con.Connections[i];
                if (connection.Auto)
                {
                    logger.Warning("Skipping connection parameters for [{0}]:{1}", i, connection.Name);
                }
                else if (connection.queueTypeName == null)
                {
                    logger.Error("Connection has not auto specific property, ignored: {0}", i);
                }
                else if (connection.QueueParameters == null)
                {
                    logger.Error("Connection parameters for queue: {0} is absent, this queue will be ignored", connection.Name);
                }
                else
                {
                    var qinterface = broker.QueueInterfaces.GetQueue(connection.queueTypeName);
                    QueueSpecificParameters parameters = qinterface.GetParametersModel();
                    parameters.SetHolder(connection.QueueParameters);
                    broker.RegisterConnection(connection.Name, qinterface, parameters, false);
                }
            }

            foreach (var channel in con.Channels)
            {
                try
                {
                    if (channel.Auto)
                    {
                        logger.Warning("Skipping channel for {0}", channel.Name);
                    }
                    else
                        broker.RegisterChannel(channel.connectionName, channel.Name, false);
                }
                catch (Exception e)
                {
                    logger.Exception(e, "Channels configuration applying, ignored");
                }
            }
            for (int i = 0; i < con.Tasks.Length; i++)
            {
                var task = con.Tasks[i];
                if (task == null)
                {
                    logger.Error("Task has not auto specific property, ignored: {0}", i);
                }
                else if (!task.Auto)
                {
                    try
                    {
                        broker.RegisterTask(task.ChannelName, task.ModuleName, task.intervalType, task.intervalValue, task.parameters, task.Description);
                    }
                    catch (Exception e)
                    {
                        logger.Exception(e, "Tasks configuration applying, ignored");
                    }
                }
            }
        }
        public static void Apply(this ConfigurationAssemblys con, Broker broker)
        {
            logger.Debug("Trying to apply ---assembly's configuration---\r\n with datetime stamp: {0} {1}", con.CreationDate.ToLongDateString(), con.CreationDate.ToLongTimeString());

            foreach (var assemblyProject in con.Assemblys)
            {
                broker.AddAssembly(assemblyProject.Name, assemblyProject.BuildServerType, assemblyProject.BSParameters);
            }

        }
    }
}
