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
        
        [FieldDescription(Ignore = true, Inherited = true, Required = false)]
        public bool Processed { get; set; }
        
        [FieldDescription(Ignore = true, Inherited = true, Required = false)]
        public DateTime AddedTime { get; set; }
        
        [FieldDescription(Ignore = true, Inherited = true, Required = false)]
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
    }
}
