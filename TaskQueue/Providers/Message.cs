using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskQueue.Providers
{
    public class TaskMessage : ITItem
    {
        public ValueMap<string, TItemValue> Holder = new ValueMap<string, TItemValue>();

        public string MType { get; private set; }

        public TaskMessage(string mtype)
        {
            MType = mtype;
        }
        public TaskMessage(Dictionary<string, TaskQueue.TItemValue> msgDict)
        {
            Type t = this.GetType();
            QueueItemModel model = new QueueItemModel(t);
            foreach (KeyValuePair<string, TItemValue> v in msgDict)
            {
                Holder.Add(v.Key, v.Value);
                PropertyInfo pi = t.GetProperty(v.Key);
                if (pi == null)
                    continue;
                pi.SetValue(this, v.Value.Value, null);
            }
        }

        public List<string> EnumerateKeys()
        {
            SetProps();
            return Holder.val1;
        }
        void ReverseSetProps()
        {
            Type t = this.GetType();
            QueueItemModel model = new QueueItemModel(t);
            for (int i = 0; i < Holder.val1.Count; i++)
            {
                PropertyInfo pi = t.GetProperty(Holder.val1[i]);
                if (pi == null)
                    continue;
                pi.SetValue(this, Holder.val2[i].Value, null);
            }
        }

        void SetProps()
        {
            Holder.Clear();

            Type t = this.GetType();
            QueueItemModel model = new QueueItemModel(t);
            foreach (string k in model.schema.val1)
            {
                PropertyInfo pi = t.GetProperty(k);
                object val = pi.GetValue(this, null);
                bool nullable;
                Holder.Add(k, new TItemValue()
                    {
                        Type = QueueItemModel.GetRType(pi.PropertyType, out nullable),
                        Value = val
                    });
            }
        }


        public bool Processed
        {
            get;
            set;
        }

        public DateTime AddedTime
        {
            get;
            set;
        }

        public DateTime? ProcessedTime
        {
            get;
            set;
        }

        public IEnumerable<ValueMapItem<string, TItemValue>> GetValues()
        {
            SetProps();
            return Holder.ALL();
        }
    }
}
