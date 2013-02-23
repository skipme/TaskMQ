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
            ChannelAnteroom ante = GetByName(mc.UniqueName);
            try
            {
                ante.Queue.OptimiseForSelector(mc.consumerSelector);
            }
            catch (Exception e)
            {
                Console.WriteLine("e {0}, {1}", e.Message, e.StackTrace);
            }
        }
        public ChannelAnteroom GetByName(string mtName)
        {
            ChannelAnteroom anteroom = null;
            MessageChannel mc = (from mm in MessageChannels
                                 where mm.UniqueName.Equals(mtName, StringComparison.OrdinalIgnoreCase)
                                 select mm).FirstOrDefault();

            if (Anterooms.ContainsKey(mtName))
            {
                anteroom = Anterooms[mtName];
            }
            else
            {
                anteroom = new ChannelAnteroom(mc.consumerSelector);
                anteroom.Queue = Queues.GetQueue(mc.QueueName);
                TaskQueue.Providers.QueueConnectionParameters qparams = Connections.GetByName(mc.ConnectionParameters);
                try
                {
                    anteroom.Queue.InitialiseFromModel(mc.MessageModel.Model, qparams);
                    Anterooms.Add(mtName, anteroom);
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
