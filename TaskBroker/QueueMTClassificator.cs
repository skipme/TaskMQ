﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue;
using TaskQueue.Providers;
using TaskUniversum;

namespace TaskBroker
{
    public class MessageTypeClassificator
    {
        ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        public Dictionary<string, ChannelAnteroom> Anterooms;

        readonly object channelSync = new object();

        public readonly List<MessageChannel> MChannelsList;
        public readonly Dictionary<string, int> MessageChannels;
        public readonly Dictionary<string, int> MessageTypes;

        public readonly Dictionary<string, QueueConnectionParameters> Connections;

        public MessageTypeClassificator()
        {
            MChannelsList = new List<MessageChannel>();
            MessageChannels = new Dictionary<string, int>();
            MessageTypes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            Anterooms = new Dictionary<string, ChannelAnteroom>();
            Connections = new Dictionary<string, QueueConnectionParameters>();
        }
        public void AddConnection(QueueConnectionParameters conParameters)
        {
            Connections.Add(conParameters.Name, conParameters);
        }
        /// <summary>
        /// assume now for one message type we have only one channel
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="m"></param>
        /// <param name="moduleName">For exception information if occured model conflicts</param>
        public void AssignMessageTypeToChannel(string channelName, TItemModel m, string moduleName)
        {
            int v;
            if (MessageChannels.TryGetValue(channelName, out v))
            {
                MessageChannel channel = MChannelsList[v];
                if (channel.AssignedMessageModel == null)
                {
                    channel.FirstModuleNameAssigned = moduleName;
                    channel.AssignedMessageType = m.ItemTypeName;
                    channel.AssignedMessageModel = m;
                    //if (!MessageTypes.ContainsKey(m.ItemTypeName))
                    MessageTypes.Add(m.ItemTypeName, v);
                }
            }
            else
            {
                throw new Exception("Error: assign message type model to channel: channel name " + channelName + "not exists");
            }
        }

        public void AddMessageChannel(MessageChannel mc)
        {
            lock (channelSync)
            {
                MessageChannels.Add(mc.UniqueName, MChannelsList.Count);
                //MessageChannelsModels.Add(mc.MessageType, MChannelsList.Count);
                MChannelsList.Add(mc);
            }
            ChannelAnteroom ante = GetAnteroom(mc.UniqueName);
            try
            {
                ante.Queue.OptimiseForSelector();
            }
            catch (Exception e)
            {
                logger.Exception(e, "OptimiseForSelector", "error in selector optimisation");
            }
        }
        public MessageChannel GetChannelForMessage(string mtName)
        {
            int v;
            if (MessageTypes.TryGetValue(mtName, out v))
            {
                return MChannelsList[v];
            }
            return null;
        }
        public ChannelAnteroom GetAnteroomByMessage(string mtName)
        {
            MessageChannel mc = GetChannelForMessage(mtName);
            if (mc == null)
                return null;
            return GetAnteroom(mc.UniqueName);
        }
        public MessageChannel GetInstanceByName(string name)
        {
            int chId = 0;
            if (MessageChannels.TryGetValue(name, out chId))
            {
                return MChannelsList[chId];
            }
            return null;
        }
        public ChannelAnteroom GetAnteroom(string name)
        {
            ChannelAnteroom anteroom = null;
            if (Anterooms.TryGetValue(name, out anteroom))
            {
                return anteroom;
            }
            else
            {
                MessageChannel mc = GetInstanceByName(name);
                anteroom = new ChannelAnteroom();
                anteroom.ChannelName = name;

                TaskQueue.Providers.QueueConnectionParameters qparams = Connections[mc.ConnectionName];
                anteroom.Queue = qparams.QueueInstance;
                //anteroom.Queue = (TaskQueue.ITQueue)Activator.CreateInstance(qparams.QueueInstance.GetType()); //Queues.GetQueue(mc.QueueName);
                try
                {
                    anteroom.Queue.InitialiseFromModel(new RepresentedModel(typeof(TaskMessage)), qparams);// schema free only queue providers (mongodb)
                    // set selector to queue
                    anteroom.Queue.SetSelector(mc.consumerSelector);
                }
                catch (QueueConnectionException e)
                {
                    logger.Warning(e.Message);
                }
                catch (Exception e)
                {
                    logger.Exception(e, "Anterooms.Add", "anteroom initialisation error");
                }

                Anterooms.Add(name, anteroom);

                return anteroom;
            }

            return null;
        }

        public IEnumerable<Statistics.BrokerStat> GetStatistics(bool input = false)
        {
            foreach (KeyValuePair<string, ChannelAnteroom> sca in Anterooms)
            {
                yield return input ? sca.Value.ChannelStatsIn : sca.Value.ChannelStatsOut;
            }
        }
    }
}
