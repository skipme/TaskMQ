using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue.Providers
{
    /// <summary>
    /// Now support only fifo queue - without update, custom selectors
    /// </summary>
    public class MemQueue : ITQueue
    {
        RepresentedModel m { get; set; }
        string CollectionName { get; set; }
        Queue<Providers.TaskMessage> baseQueue;

        public MemQueue()
        {
            baseQueue = new Queue<Providers.TaskMessage>();
        }
        public MemQueue(RepresentedModel model, QueueConnectionParameters connection)
        {
            this.InitialiseFromModel(model, connection);
        }

        public void Push(Providers.TaskMessage item)
        {
            baseQueue.Enqueue(item);
        }

        public Providers.TaskMessage GetItemFifo()
        {
            if (baseQueue.Count == 0)
                return null;
            return baseQueue.Dequeue();
        }

        public Providers.TaskMessage GetItem()
        {
            if (baseQueue.Count == 0)
                return null;
            return baseQueue.Dequeue();
        }

        public void InitialiseFromModel(RepresentedModel model, QueueConnectionParameters connection)
        {
            this.m = model;
            CollectionName = connection.Collection;
        }

        public string QueueType
        {
            get { return "InMemoryQueue"; }
        }


        public string QueueDescription
        {
            get { return "Simple dynamic in-memory queue"; }
        }

        public void UpdateItem(Providers.TaskMessage item)
        {
            throw new NotImplementedException();
        }


        public void OptimiseForSelector()
        {
            throw new NotImplementedException();
        }

        public Providers.TaskMessage[] GetItemTuple()
        {
            if (baseQueue.Count >= 1)
            {
                return new TaskMessage[]{
                    baseQueue.Dequeue()
                };
            }
            else
            {
                return null;
            }
        }

        public long GetQueueLength()
        {
            return baseQueue.Count;
        }

        public void SetSelector(TQItemSelector selector = null)
        {
            throw new NotImplementedException();
        }
    }
}
