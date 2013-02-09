using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskQueue.Providers
{
    public class TaskMessage : ITItem
    {
        public Dictionary<string, object> Holder = new Dictionary<string, object>();

        public string MType { get; set; }
        public bool Processed { get; set; }
        public DateTime AddedTime { get; set; }
        public DateTime? ProcessedTime { get; set; }

        public TaskMessage(string mtype)
        {
            MType = mtype;
        }
        public TaskMessage(Dictionary<string, object> msgDict)
        {
            Type t = this.GetType();
            QueueItemModel model = new QueueItemModel(t);
            foreach (KeyValuePair<string, object> v in msgDict)
            {
                PropertyInfo pi = t.GetProperty(v.Key);
                if (pi == null)
                    continue;
                Type pt = pi.PropertyType;

                bool nullable = false;
                Type ptn = Nullable.GetUnderlyingType(pt);
                if (nullable = ptn != null)
                    pt = ptn;

                if (v.Value == null && !nullable)
                    throw new Exception("value does not accept nullable types, key: " + v.Key);
                else if (!nullable &&pt != v.Value.GetType())
                    throw new Exception("unkown type for key: " + v.Key);

                pi.SetValue(this, v.Value, null);
            }
            Holder = msgDict;
        }
        void ReverseSetProps()
        {
            Type t = this.GetType();
            foreach (KeyValuePair<string, object> v in Holder)
            {
                PropertyInfo pi = t.GetProperty(v.Key);
                if (pi == null)
                    continue;
                pi.SetValue(this, v.Value, null);
            }
        }

        void SetProps()
        {
            Type t = this.GetType();
            QueueItemModel model = new QueueItemModel(t);
            foreach (string k in model.schema.val1)
            {
                PropertyInfo pi = t.GetProperty(k);
                object val = pi.GetValue(this, null);

                if (Holder.ContainsKey(k))
                    Holder[k] = val;
                else
                    Holder.Add(k, val);
            }
        }

        public dynamic ToExpando()
        {
            SetProps();
            var result = new ExpandoObject();
            var d = result as IDictionary<string, object>; //work with the Expando as a Dictionary
            d = Holder;
            return result;
        }
        public Dictionary<string, object> ToDictionary()
        {
            SetProps();
            return Holder;
        }

        public Dictionary<string, object> GetHolder()
        {
            SetProps();
            return Holder;
        }
    }
}
