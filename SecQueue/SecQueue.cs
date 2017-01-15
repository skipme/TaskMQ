using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue;
using TaskQueue.Providers;

namespace SecQueue
{
    /// <summary>
    /// In-memory queue based on SecSortedSet
    /// </summary>
    public class SecQueue : ITQueue
    {
        public class EncapsulatedMessageComparer : IComparer<TaskMessage>
        {
            public MessageComparer internalComparer;
            public EncapsulatedMessageComparer(MessageComparer msgComparer)
            {
                internalComparer = msgComparer;
            }

            public int Compare(TaskMessage x, TaskMessage y)
            {
                int r = internalComparer.Compare(x, y);
                if (r == 0)
                    return ((int)x.Holder["__idx"]).CompareTo((int)y.Holder["__idx"]);
                return r;
            }
        }
        public const string queueTypeName = "InMemoryQueue";

        const int maxTuple = 100;
        RepresentedModel m { get; set; }
        EncapsulatedMessageComparer comparer;

        int Counter = 0;
        SecSortedSet MessageQueue;

        public string Name;

        public SecQueue()
        {

        }
        public SecQueue(RepresentedModel model, QueueConnectionParameters connection)
        {
            this.InitialiseFromModel(model, connection);
            Name = connection.Name;
        }

        public void Push(TaskMessage item)
        {
            try
            {
                if (item.Holder == null) item.GetHolder();
                if (comparer.internalComparer.Check(item))
                {
                    lock (MessageQueue)
                    {
                        item.Holder["__idx"] = Counter;
                        MessageQueue.Add(item);
                        Counter++;
                    }
                }

            }
            catch (OutOfMemoryException excOverfl)
            {
                throw new QueueOverflowException(excOverfl);
            }
        }

        public TaskMessage GetItem()
        {
            if (MessageQueue.Count == 0)
                return null;
            lock (MessageQueue)
            {
                TaskMessage result;

                result = MessageQueue.GetMinKey();
                TaskMessage msg = new TaskMessage(result.Holder);
                msg.Holder.Add("__original", result);
                return msg;
            }
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

        public void UpdateItem(TaskMessage item)
        {
            Dictionary<string, object> holder = item.GetHolder();
            object id = holder["__original"];
            if (id == null || !(id is TaskMessage))
                throw new Exception("__original of queue element is missing");
            TaskMessage orig = (TaskMessage)id;
            holder.Remove("__original");
            lock (MessageQueue)
            {
                if (!MessageQueue.Remove(orig))
                    throw new Exception("can't update element in queue");
                this.Push(item);
            }
        }


        public void OptimiseForSelector()
        {
            //throw new NotImplementedException();
        }

        public TaskMessage[] GetItemTuple()
        {
            //lock (MessageQueue)
            //    if (MessageQueue.Count > 0)
            //    {
            //        TaskMessage[] tuple = null;
            //        if (maxTuple < MessageQueue.Count)
            //        {
            //            tuple = new TaskMessage[maxTuple];
            //            MessageQueue.CopyTo(tuple, 0, tuple.Length);
            //        }
            //        else
            //        {
            //            tuple = new TaskMessage[MessageQueue.Count];
            //            MessageQueue.CopyTo(tuple);
            //        }
            //        for (int i = 0; i < tuple.Length; i++)
            //        {
            //            TaskMessage em = tuple[i];
            //            TaskMessage msg = new TaskMessage(em.Holder);
            //            msg.Holder.Add("__original", em);
            //            tuple[i] = msg;
            //        }
            //        return tuple;

            //    }
            //    else
            //    {
            //        return null;
            //    }
            return null;

        }

        public long GetQueueLength()
        {
            //return baseQueue.Count;
            return MessageQueue.Count;
        }

        public void SetSelector(TQItemSelector selector = null)
        {
            if (selector == null)
                selector = TQItemSelector.DefaultFifoSelector;
            comparer = new EncapsulatedMessageComparer(new MessageComparer(selector));
            MessageQueue = new SecSortedSet(comparer/*, 1000, 1000*/);
        }

        public QueueSpecificParameters GetParametersModel()
        {
            return new MemQueueParams();
        }
    }
    public class SecQueueParams : QueueSpecificParameters
    {
        //[TaskQueue.FieldDescription("Flush/restore queue to disk", Required = true, DefaultValue = true)]
        //public bool Persistant { get; set; }

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
