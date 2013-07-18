using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TQModelProp : Attribute
    {
        public string Description;
        public bool Ignore { get; set; }

        public TQModelProp(string Description)
        {
            this.Description = Description;
        }
        public TQModelProp(bool ignore)
        {
            this.Ignore = ignore;
        }
    }
    public class QueueItemModelValue
    {
        public QueueItemModelValue(TItemValue_Type VType)
        {
            this.VType = VType;
        }
        public TItemValue_Type VType { get; set; }
        public string Description { get; set; }
    }
}
