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
            //addQueue(new MSSQLQueue.MsSqlQueue());
            addQueue(new MongoQueue.MongoDbQueue());
        }
        // queues
        void addQueue(TaskQueue.ITQueue q)
        {
            QueueList.Add(q.QueueType, q);
        }

        public TaskQueue.ITQueue GetQueue(string name)
        {
            TaskQueue.ITQueue q = (TaskQueue.ITQueue)Activator.CreateInstance(QueueList[name].GetType());
            return q;
        }

        public Dictionary<string, TaskQueue.ITQueue> QueueList = new Dictionary<string, TaskQueue.ITQueue>();
    }
}
