using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker
{
    public class QueueMTClassificator
    {
        public QueueMTClassificator()
        {
            MessageModels = new List<MessageType>();
            MessageChannels = new List<MessageChannel>();
            Anterooms = new Dictionary<string, ChannelAnteroom>();
            Queues = new QueueClassificator();
            Connections = new QueueConParams();
        }
        public void AddMessageType(MessageType mt)
        {
            MessageModels.Add(mt);
        }
        public void AddMessageChannel(MessageChannel mc, string messageModelName)
        {
            MessageChannels.Add(mc);
            mc.MessageModel = (from mm in MessageModels
                               where mm.UniqueName.Equals(messageModelName, StringComparison.OrdinalIgnoreCase)
                               select mm).FirstOrDefault();
            ChannelAnteroom ante = GetAnteroom(mc.UniqueName);
            try
            {
                ante.Queue.OptimiseForSelector(mc.consumerSelector);
            }
            catch (Exception e)
            {
                Console.WriteLine("e {0}, {1}", e.Message, e.StackTrace);
            }
        }
        public MessageChannel GetChannelForMessage(string mtName)
        {
            // first only // ballancer to do
            MessageChannel mc = (from mm in MessageChannels
                                 where mm.MessageModel.UniqueName.Equals(mtName, StringComparison.OrdinalIgnoreCase)
                                 select mm).FirstOrDefault();
            return mc;
        }
        public ChannelAnteroom GetAnteroomForMessage(string mtName)
        {
            MessageChannel mc = GetChannelForMessage(mtName);

            return GetAnteroom(mc.UniqueName);
        }
        public MessageChannel GetChannel(string name)
        {
            MessageChannel mc = (from mm in MessageChannels
                                 where mm.UniqueName.Equals(name, StringComparison.OrdinalIgnoreCase)
                                 select mm).FirstOrDefault();
            return mc;
        }
        public ChannelAnteroom GetAnteroom(string name)
        {
            ChannelAnteroom anteroom = null;
            MessageChannel mc = GetChannel(name);

            if (Anterooms.ContainsKey(name))
            {
                anteroom = Anterooms[name];
            }
            else
            {
                anteroom = new ChannelAnteroom(mc.consumerSelector);
                anteroom.ChannelName = name;
                anteroom.Queue = Queues.GetQueue(mc.QueueName);
                TaskQueue.Providers.QueueConnectionParameters qparams = Connections.GetByName(mc.ConnectionParameters);
                try
                {
                    anteroom.Queue.InitialiseFromModel(mc.MessageModel.Model, qparams);
                    Anterooms.Add(name, anteroom);
                }
                catch (Exception e)
                {
                    Console.WriteLine("e {0}, {1}", e.Message, e.StackTrace);
                }
            }

            return anteroom;
        }
        public Dictionary<string, ChannelAnteroom> Anterooms;
        public List<MessageType> MessageModels;
        public List<MessageChannel> MessageChannels;
        public QueueClassificator Queues;
        public QueueConParams Connections;
    }
}
