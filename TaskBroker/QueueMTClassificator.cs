using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue;
using TaskQueue.Providers;

namespace TaskBroker
{
    public class QueueMTClassificator
    {
        public QueueMTClassificator()
        {
            //MessageModels = new List<MessageType>();
            MessageChannels = new List<MessageChannel>();
            Anterooms = new Dictionary<string, ChannelAnteroom>();
            Queues = new QueueClassificator();
            //Connections = new QueueConParams();
            Connections = new Dictionary<string, QueueConnectionParameters>();
        }
        public void AddConnection(QueueConnectionParameters conParameters)
        {
            //Connections.Add(conParameters);
            Connections.Add(conParameters.Name, conParameters);
        }
        public void AddMessageType(MessageType mt)
        {
            //MessageModels.Add(mt);
        }
        public void AddMessageChannel<T>(MessageChannel mc)
            where T : TaskQueue.Providers.TItemModel
        {
            MessageChannels.Add(mc);

            mc.MessageModel = Activator.CreateInstance<T>();

            //(from mm in MessageModels
            //               where mm.UniqueName.Equals(messageModelName, StringComparison.OrdinalIgnoreCase)
            //               select mm).FirstOrDefault();
            ChannelAnteroom ante = GetAnteroom(mc.UniqueName);
            try
            {
                ante.Queue.OptimiseForSelector(mc.consumerSelector);
            }
            catch (Exception e)
            {
                Console.WriteLine("error in selector optimisation {0}, {1}", e.Message, e.StackTrace);
            }
        }
        public MessageChannel GetChannelForMessage(string mtName)
        {
            // first only // ballancer to do
            MessageChannel mc = (from mm in MessageChannels
                                 where mm.MessageModel.ItemTypeName.Equals(mtName, StringComparison.OrdinalIgnoreCase)
                                 select mm).FirstOrDefault();
            return mc;
        }
        public ChannelAnteroom GetAnteroomByMessage(string mtName)
        {
            MessageChannel mc = GetChannelForMessage(mtName);

            return GetAnteroom(mc.UniqueName);
        }
        public MessageChannel GetChannelByName(string name)
        {
            MessageChannel mc = (from mm in MessageChannels
                                 where mm.UniqueName.Equals(name, StringComparison.OrdinalIgnoreCase)
                                 select mm).FirstOrDefault();
            return mc;
        }
        public ChannelAnteroom GetAnteroom(string name)
        {
            if (Anterooms.ContainsKey(name))
            {
                return Anterooms[name];
            }
            else
            {
                MessageChannel mc = GetChannelByName(name);
                ChannelAnteroom anteroom = new ChannelAnteroom(mc.consumerSelector);
                anteroom.ChannelName = name;

                TaskQueue.Providers.QueueConnectionParameters qparams = Connections[mc.ConnectionName];
                anteroom.Queue = (TaskQueue.ITQueue)Activator.CreateInstance(qparams.QueueInstance.GetType()); //Queues.GetQueue(mc.QueueName);
                try
                {
                    anteroom.Queue.InitialiseFromModel(new QueueItemModel(mc.MessageModel.GetType()), qparams);
                    Anterooms.Add(name, anteroom);

                    return anteroom;
                }
                catch (Exception e)
                {
                    Console.WriteLine("e {0}, {1}", e.Message, e.StackTrace);
                }
            }

            return null;
        }
        public Dictionary<string, ChannelAnteroom> Anterooms;
        //public List<MessageType> MessageModels;
        public List<MessageChannel> MessageChannels;
        public QueueClassificator Queues;
        public Dictionary<string, QueueConnectionParameters> Connections;
        //public QueueConParams Connections;
    }
}
