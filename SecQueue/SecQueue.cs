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
        public class EncapsulatedMessageComparer : IComparer<ValueMap<string, object>>
        {
            public MessageComparerVMap internalComparer;
            public EncapsulatedMessageComparer(MessageComparerVMap msgComparer)
            {
                internalComparer = msgComparer;
            }

            public int Compare(ValueMap<string, object> x, ValueMap<string, object> y)
            {
                int r = internalComparer.Compare(x, y);
                if (r == 0)
                    return ((int)x.val2[x.val2.Count - 1]).CompareTo((int)y.val2[y.val2.Count - 1]);
                //return ((int)x.Holder["__idx"]).CompareTo((int)y.Holder["__idx"]);
                return r;
            }
        }
        public const string queueTypeName = "InMemoryQueue";

        const int maxTuple = 100;
        RepresentedModel m { get; set; }
        EncapsulatedMessageComparer comparer;

        int Counter = 0;
        readonly object queueSync = new object();
        SecSortedSet MessageQueue;
        TQItemSelector selector;
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
                ValueMap<string, object> vmap = item.GetValueMap(this.selector);
                if (comparer.internalComparer.Check(item.Holder))
                {
                    lock (queueSync)
                    {
                        vmap.Add("__idx", Counter);
                        MessageQueue.Add(vmap);
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
            lock (queueSync)
            {
                ValueMap<string, object> result;

                result = MessageQueue.GetMinKey();
                TaskMessage msg = new TaskMessage(result.ToDictionary());
                msg.Holder.Remove("__idx");
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
            if (id == null || !(id is ValueMap<string, object>))
                throw new Exception("__original of queue element is missing");
            ValueMap<string, object> orig = (ValueMap<string, object>)id;
            holder.Remove("__original");
            lock (queueSync)
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
            //lock (queueSync)
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
            this.selector = selector;
            comparer = new EncapsulatedMessageComparer(new MessageComparerVMap(selector));
            MessageQueue = new SecSortedSet(comparer, 64, 64);
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
