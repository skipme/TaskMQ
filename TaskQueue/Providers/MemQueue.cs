using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue.Providers
{
    public class MemQueue : ITQueue
    {
        public static Dictionary<string, Queue<ITItem>> collections = new Dictionary<string, Queue<ITItem>>();
       
        QueueItemModel m;
        Queue<ITItem> baseQueue
        {
            get
            {
                if (collections.ContainsKey(m.CollectionName))
                    return collections[m.CollectionName];
                collections.Add(m.CollectionName, new Queue<ITItem>());
                return collections[m.CollectionName];
            }
        }
        public MemQueue()
        {
        }
        public MemQueue(QueueItemModel model)
        {
            this.InitialiseFromModel(model);
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

        public void InitialiseFromModel(QueueItemModel model)
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
