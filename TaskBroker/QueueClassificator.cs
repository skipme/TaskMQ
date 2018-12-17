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
            AddQueueType(new TaskQueue.Providers.MemQueue());
            //addQueue(new MSSQLQueue.MsSqlQueue());
            AddQueueType(new MongoQueue.MongoDbQueue());
        }
        // queues
        public void AddQueueType(TaskQueue.ITQueue q)
        {
            QueueList.Add(q.QueueType, q);
        }
        /// <summary>
        /// Create new instance of Queue
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TaskQueue.ITQueue GetQueue(string name)
        {
            TaskQueue.ITQueue q = (TaskQueue.ITQueue)Activator.CreateInstance(QueueList[name].GetType());
            return q;
        }

        public Dictionary<string, TaskQueue.ITQueue> QueueList = new Dictionary<string, TaskQueue.ITQueue>();
    }
}
