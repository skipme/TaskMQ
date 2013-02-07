using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace TaskBroker
{
    public class MessageType
    {
        public string Name { get; set; }
        public TaskQueue.QueueItemModel Model { get; set; }
        public TaskQueue.ITQueue Queue { get; set; }
        public string Collection { get; set; }
        public string ConnectionString { get; set; }
    }
}
