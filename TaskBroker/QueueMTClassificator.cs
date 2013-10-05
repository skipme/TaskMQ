using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue;
using TaskQueue.Providers;

namespace TaskBroker
{
    public class MessageTypeClassificator
    {
        public MessageTypeClassificator()
        {
            MChannelsList = new List<MessageChannel>();
            MessageChannels = new Dictionary<string, int>();
            MessageChannelsModels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            Anterooms = new Dictionary<string, ChannelAnteroom>();
            Connections = new Dictionary<string, QueueConnectionParameters>();
        }
        public void AddConnection(QueueConnectionParameters conParameters)
        {
            Connections.Add(conParameters.Name, conParameters);
        }
        /// <summary>
        /// assume now for message type we have only one channel
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
                channel.FirstModuleNameAssigned = moduleName;
                channel.AssignedMessageType = m.ItemTypeName;
                channel.AssignedMessageModel = m;
                MessageChannelsModels.Add(m.ItemTypeName, v);
            }
            else
            {
                throw new Exception("-> Error: assign message type model to channel: channel name " + channelName + "not exists");
            }

        }
        //public void AddMessageChannel<T>(MessageChannel mc)
        //    where T : TaskQueue.Providers.TItemModel
        //{
        //    TaskQueue.Providers.TItemModel tim = Activator.CreateInstance<T>();
        //    //mc.MessageType = tim.ItemTypeName;
        //    lock (MChannelsList)
        //    {
        //        MessageChannels.Add(mc.UniqueName, MChannelsList.Count);
        //        //MessageChannelsModels.Add(mc.MessageType, MChannelsList.Count);
        //        MChannelsList.Add(mc);
        //    }
        //    ChannelAnteroom ante = GetAnteroom(mc.UniqueName);
        //    try
        //    {
        //        ante.Queue.OptimiseForSelector(mc.consumerSelector);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("-> error in selector optimisation {0}, {1}", e.Message, e.StackTrace);
        //    }
        //}
        public void AddMessageChannel(MessageChannel mc)
        {
            lock (MChannelsList)
            {
                MessageChannels.Add(mc.UniqueName, MChannelsList.Count);
                //MessageChannelsModels.Add(mc.MessageType, MChannelsList.Count);
                MChannelsList.Add(mc);
            }
            ChannelAnteroom ante = GetAnteroom(mc.UniqueName);
            try
            {
                ante.Queue.OptimiseForSelector(mc.consumerSelector);
            }
            catch (Exception e)
            {
                Console.WriteLine("-> error in selector optimisation {0}, {1}", e.Message, e.StackTrace);
            }
        }
        public MessageChannel GetChannelForMessage(string mtName)
        {
            int v;
            if (MessageChannelsModels.TryGetValue(mtName, out v))
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
        public MessageChannel GetChannelByName(string name)
        {
            //MessageChannel mc = (from mm in MessageChannels
            //                     where mm.UniqueName.Equals(name, StringComparison.OrdinalIgnoreCase)
            //                     select mm).FirstOrDefault();
            //return mc;
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
                MessageChannel mc = GetChannelByName(name);
                anteroom = new ChannelAnteroom(mc.consumerSelector);
                anteroom.ChannelName = name;

                TaskQueue.Providers.QueueConnectionParameters qparams = Connections[mc.ConnectionName];
                anteroom.Queue = (TaskQueue.ITQueue)Activator.CreateInstance(qparams.QueueInstance.GetType()); //Queues.GetQueue(mc.QueueName);
                try
                {
                    //anteroom.Queue.InitialiseFromModel(new QueueItemModel(mc.MessageModel.GetType()), qparams);
                    anteroom.Queue.InitialiseFromModel(new RepresentedModel(typeof(TaskMessage)), qparams);// schema free only queue providers (exclude sql server)
                    Anterooms.Add(name, anteroom);

                    return anteroom;
                }
                catch (Exception e)
                {
                    Console.WriteLine("-> anteroom initialisation error: {0}, {1}", e.Message, e.StackTrace);
                }
            }

            return null;
        }

        public IEnumerable<Statistics.BrokerStat> GetStatistics()
        {
            foreach (KeyValuePair<string, ChannelAnteroom> sca in Anterooms)
            {
                yield return sca.Value.ChannelStatistic;
            }
        }
        public Dictionary<string, ChannelAnteroom> Anterooms;

        public List<MessageChannel> MChannelsList;
        public Dictionary<string, int> MessageChannels;
        public Dictionary<string, int> MessageChannelsModels;

        public Dictionary<string, QueueConnectionParameters> Connections;
    }
}
