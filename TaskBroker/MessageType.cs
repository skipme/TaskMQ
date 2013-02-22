using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace TaskBroker
{
    public class MessageType
    {
        public MessageType() { consumerSelector = TaskQueue.TQItemSelector.DefaultFifoSelector; }
        public string UniqueName { get; set; }
        public TaskQueue.QueueItemModel Model { get; set; }
        public string QueueName { get; set; }
        public TaskQueue.TQItemSelector consumerSelector { get; set; }

        public string ConnectionParameters { get; set; }
    }
}
