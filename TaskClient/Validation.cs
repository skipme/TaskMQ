using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskClient
{
    public class Validation
    {
        public class ValidationRepresentedModelValue
        {
            public ValidationRepresentedModelValue(TaskQueue.RepresentedModelValue refmod)
            {
                this.VType = refmod.VType;
                this.Description = refmod.Description;
                this.Required = refmod.Required;
                this.Inherited = refmod.Inherited;
                this.DefaultValue = refmod.DefaultValue;
            }
            public TaskQueue.FieldType VType { get; set; }
            public string Description { get; set; }
            public bool Required { get; set; }
            public bool Inherited { get; set; }
            public object DefaultValue { get; set; }

            public TaskQueue.RepresentedModelValue toNative()
            {
                TaskQueue.RepresentedModelValue r = new TaskQueue.RepresentedModelValue(this.VType);

                r.Description = this.Description;
                r.Required = this.Required;
                r.Inherited = this.Inherited;
                r.DefaultValue = this.DefaultValue;
                return r;
            }
        }

        public class ValidationRequest
        {
            public string MType { get; set; }
            public string ChannelName { get; set; }
        }
        public class ValidationResponse
        {
            public Dictionary<string, TaskQueue.RepresentedModelValue> NativeScheme
            {
                get
                {
                    Dictionary<string, TaskQueue.RepresentedModelValue> conv = new Dictionary<string, TaskQueue.RepresentedModelValue>();
                    foreach (KeyValuePair<string, TaskClient.Validation.ValidationRepresentedModelValue> item in ModelScheme)
                    {
                        conv.Add(item.Key, item.Value.toNative());
                    }
                    return conv;
                }
            }
            public Dictionary<string, TaskClient.Validation.ValidationRepresentedModelValue> ModelScheme { get; set; }
        }
    }
}
