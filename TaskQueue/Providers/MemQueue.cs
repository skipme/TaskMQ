using System;
using System.Collections.Generic;

using System.Text;

namespace TaskQueue.Providers
{
    /// <summary>
    /// Now support only fifo queue - without update, custom selectors
    /// </summary>
    public class MemQueue : ITQueue
    {
        public const string queueTypeName = "InMemoryQueue";

        const int maxTuple = 100;
        RepresentedModel m { get; set; }

        Queue<Providers.TaskMessage> baseQueue;
        public string Name;

        public MemQueue()
        {
            baseQueue = new Queue<Providers.TaskMessage>();
            //DateTime dt = DateTime.UtcNow;
            //Providers.TaskMessage[] tarrtp = new Providers.TaskMessage[2000000];
            //for (int i = 0; i < tarrtp.Length; i++)
            //{
            //    tarrtp[i] = new TaskMessage("benchMessage")
            //    {
            //        AddedTime = dt
            //    };
            //    dt = dt.AddSeconds(1);
            //}
            //baseQueue = new Queue<Providers.TaskMessage>(tarrtp);
        }
        public MemQueue(RepresentedModel model, QueueConnectionParameters connection)
        {
            this.InitialiseFromModel(model, connection);
            Name = connection.Name;
        }

        public void Push(Providers.TaskMessage item)
        {
            try
            {
                lock (baseQueue)
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
            lock (baseQueue)
                return baseQueue.Dequeue();
        }

        public Providers.TaskMessage GetItem()
        {
            if (baseQueue.Count == 0)
                return null;
            lock (baseQueue)
                return baseQueue.Dequeue();
        }

        public void InitialiseFromModel(RepresentedModel model, QueueConnectionParameters connection)
        {
            this.m = model;
        }

        public string QueueType
        {
            get { return queueTypeName; }
        }


        public string QueueDescription
        {
            get { return "Non persistent in-memory queue"; }
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
            lock (baseQueue)
                if (baseQueue.Count > 0)
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
           // throw new NotImplementedException();
            //selector.parameters[""]
        }

        public QueueSpecificParameters GetParametersModel()
        {
            return new MemQueueParams();
        }
    }
    public class MemQueueParams : QueueSpecificParameters
    {
        [TaskQueue.FieldDescription("Flush/restore queue to disk", Required = true, DefaultValue = true)]
        public bool Persistant { get; set; }

        public override bool Validate(out string result)
        {
            result = "";
            return true;
        }
        public override string ItemTypeName
        {
            get
            {
                return this.GetType().Name;
            }
            set
            {

            }
        }
    }
}
