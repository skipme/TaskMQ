using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker
{
    public class QueueClassificator
    {
        public QueueClassificator()
        {
            addQueue(new TaskQueue.Providers.MemQueue());
            addQueue(new MSSQLQueue.MsSqlQueue());
            addQueue(new MongoQueue.MongoDbQueue());
        }
        // queues
        void addQueue(TaskQueue.ITQueue q)
        {
            q.OptimiseForSelector(TaskQueue.TQItemSelector.DefaultFifoSelector);
            QueueList.Add(q.QueueType, q);
        }

        public TaskQueue.ITQueue GetQueue(string name)
        {
            return QueueList[name];
        }

        public Dictionary<string, TaskQueue.ITQueue> QueueList = new Dictionary<string, TaskQueue.ITQueue>();
    }
}
