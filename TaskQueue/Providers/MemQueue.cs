using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue.Providers
{
    public class MemQueue : ITQueue
    {
        //public static Dictionary<string, Queue<ITItem>> collections = new Dictionary<string, Queue<ITItem>>();

        RepresentedModel m { get; set; }
        string CollectionName { get; set; }

        //Queue<ITItem> baseQueue
        //{
        //    get
        //    {
        //        if (collections.ContainsKey(CollectionName))
        //            return collections[CollectionName];
        //        collections.Add(CollectionName, new Queue<ITItem>());
        //        return collections[CollectionName];
        //    }
        //}
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
            return baseQueue.Dequeue();
        }

        public Providers.TaskMessage GetItem()
        {
            throw new NotImplementedException();
        }

        public void InitialiseFromModel(RepresentedModel model, QueueConnectionParameters connection, TQItemSelector selector = null)
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
            //throw new NotImplementedException();
        }


        public void OptimiseForSelector()
        {
            throw new NotImplementedException();
        }


        public Providers.TaskMessage[] GetItemTuple()
        {
            throw new NotImplementedException();
        }


        public long GetQueueLength()
        {
            return baseQueue.Count;
        }
    }
}
