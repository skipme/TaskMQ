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
            Anterooms = new Dictionary<string, ChannelAnteroom>();
            Queues = new QueueClassificator();
            Connections = new QueueConParams();
        }
        public void Add(MessageType mt)
        {
            MessageModels.Add(mt);
            ChannelAnteroom ante = GetByName(mt.UniqueName);
            ante.Queue.OptimiseForSelector(mt.consumerSelector);
        }
        public ChannelAnteroom GetByName(string mtName)
        {
            ChannelAnteroom anteroom = null;
            MessageType mt = (from mm in MessageModels
                              where mm.UniqueName.Equals(mtName, StringComparison.OrdinalIgnoreCase)
                              select mm).FirstOrDefault();

            if (Anterooms.ContainsKey(mtName))
            {
                anteroom = Anterooms[mtName];
            }
            else
            {
                anteroom = new ChannelAnteroom(mt.consumerSelector);
                anteroom.Queue = Queues.GetQueue(mt.QueueName);
                TaskQueue.Providers.QueueConnectionParameters qparams = Connections.GetByName(mt.ConnectionParameters);
                anteroom.Queue.InitialiseFromModel(mt.Model, qparams);

                Anterooms.Add(mtName, anteroom);
            }

            return anteroom;
        }
        public Dictionary<string, ChannelAnteroom> Anterooms;
        public List<MessageType> MessageModels;
        public QueueClassificator Queues;
        public QueueConParams Connections;
    }
}
