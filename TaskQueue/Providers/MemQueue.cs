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
        const int maxTuple = 100;
        RepresentedModel m { get; set; }
        string CollectionName { get; set; }
        Queue<Providers.TaskMessage> baseQueue;

        public MemQueue()
        {
            baseQueue = new Queue<Providers.TaskMessage>();
            //Providers.TaskMessage[] tarrtp = new Providers.TaskMessage[3000000];
            //for (int i = 0; i < tarrtp.Length; i++)
            //{
            //    tarrtp[i] = new TaskMessage("benchMessage");
            //}
            //baseQueue = new Queue<Providers.TaskMessage>(tarrtp);
        }
        public MemQueue(RepresentedModel model, QueueConnectionParameters connection)
        {
            this.InitialiseFromModel(model, connection);
        }

        public void Push(Providers.TaskMessage item)
        {
            try
            {
                baseQueue.Enqueue(item);
            }
            catch (OutOfMemoryException excOverfl)
            {
                throw new QueueOverflowException(excOverfl);
            }
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
            get { return "Non persistant in-memory queue"; }
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
                TaskMessage[] tuple;
                if (maxTuple < baseQueue.Count)
                {
                    tuple = new TaskMessage[maxTuple];
                    for (int i = 0; i < maxTuple; i++)
                    {
                        tuple[i] = baseQueue.Dequeue();
                    }
                }
                else
                {
                    tuple = baseQueue.ToArray();
                    baseQueue.Clear();
                }
                return tuple;

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
