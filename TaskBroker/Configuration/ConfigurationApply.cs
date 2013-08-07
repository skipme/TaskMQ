using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskBroker.Configuration;

namespace TaskBroker
{
    public static class ConfigurationApply
    {
        public static void Apply(this ConfigurationBroker con, Broker broker)
        {
            Console.WriteLine("trying to apply main configuration with datetime stamp: {0}", con.CreationDate);

            foreach (var connection in con.Connections)
            {
                var qinterface = broker.QueueInterfaces.GetQueue(connection.queueTypeName);
                broker.RegisterConnection(connection.Name, connection.connectionString,
                    connection.database, connection.collection, qinterface);
            }
            foreach (var channel in con.Channels)
            {
                broker.RegisterChannel(channel.connectionName, channel.Name, channel.MType);
            }
            foreach (var task in con.Tasks)
            {
                if (!task.Auto)
                {
                    broker.RegisterTask(task.ChannelName, task.ModuleName, task.intervalType, task.intervalValue, task.parameters, task.Description);
                }
            }
        }
    }
}
