using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskQueue.Providers
{
    public class TaskMessage : ITItem
    {
        ValueMap<string, TItemValue> Holder = new ValueMap<string, TItemValue>();

        public string MType { get; private set; }

        public TaskMessage(string mtype)
        {
            MType = mtype;
        }

        public List<string> EnumerateKeys()
        {
            SetProps();
            return Holder.val1;
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
                Holder.Add(k, new TItemValue()
                    {
                        Type = QueueItemModel.GetRType(pi.PropertyType),
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

        public DateTime ProcessedTime
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
