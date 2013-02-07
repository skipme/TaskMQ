using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue.Providers
{
    public class MemQueue : ITQueue
    {
        public static Dictionary<string, Queue<ITItem>> collections = new Dictionary<string, Queue<ITItem>>();

        QueueItemModel m { get; set; }
        string CollectionName { get; set; }

        Queue<ITItem> baseQueue
        {
            get
            {
                if (collections.ContainsKey(CollectionName))
                    return collections[CollectionName];
                collections.Add(CollectionName, new Queue<ITItem>());
                return collections[CollectionName];
            }
        }
        public MemQueue()
        {
        }
        public MemQueue(QueueItemModel model, string collection, string connectionString)
        {
            this.InitialiseFromModel(model, collection, connectionString);
        }

        public void Push(ITItem item)
        {
            baseQueue.Enqueue(item);
        }

        public ITItem GetItemFifo()
        {
            return baseQueue.Dequeue();
        }

        public ITItem GetItem(TQItemSelector selector)
        {
            throw new NotImplementedException();
        }

        public void InitialiseFromModel(QueueItemModel model, string collection, string connectionString)
        {
            this.m = model;
        }

        public string QueueType
        {
            get { return "InMemoryQueue"; }
        }


        public string QueueDescription
        {
            get { return "Simple dynamic in-memory queue"; }
        }
    }
}
