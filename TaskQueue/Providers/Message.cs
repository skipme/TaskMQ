using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskQueue.Providers
{
    public class TaskMessage : TItemModel
    {
        [FieldDescription(Ignore = false, Inherited = true, Required = true)]
        public string MType { get; set; }

        [FieldDescription(Ignore = false, Inherited = true, Required = false)]
        public bool Processed { get; set; }

        [FieldDescription(Ignore = false, Inherited = true, Required = false)]
        public DateTime AddedTime { get; set; }

        [FieldDescription(Ignore = false, Inherited = true, Required = false)]
        public DateTime? ProcessedTime { get; set; }

        public TaskMessage(Dictionary<string, object> holder)
            : base(holder)
        {
        }
        public TaskMessage(string mtype)
        {
            MType = mtype;
        }
        [FieldDescription(Ignore = true, Inherited = true, Required = false)]
        public override string ItemTypeName
        {
            get
            {
                return MType;
            }
            set
            {
                MType = value;// !!!
            }
        }
        /// <summary>
        /// Get dictionary holder without inherited properties, but with MType
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetSendEnvelope()
        {
            Dictionary<string, object> di = new Dictionary<string, object>();
            Type t = this.GetType();

            ValueMap<string, RepresentedModelValue> model = RepresentedModel.FindScheme(t);

            di.Add("MType", this.MType);

            for (int i = 0; i < model.val1.Count; i++)
            {
                string k = model.val1[i];
                RepresentedModelValue v = model.val2[i];
                if (!v.Inherited)
                {
                    PropertyInfo pi = model.val2[i].propertyDescriptor;
                    object val = pi.GetValue(this, null);
                    di.Add(k, val);
                }
            }
            return di;
        }
        public override string ToString()
        {
            string result = "";
            foreach (KeyValuePair<string, object> pair in GetSendEnvelope())
            {
                result += pair.Key + " = " + pair.Value.ToString() + ", \n";
            }
            return result;
        }
    }
}
