using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldDescription : Attribute
    {
        public string Description;
        public bool Ignore { get; set; }

        public FieldDescription(string Description)
        {
            this.Description = Description;
        }
        public FieldDescription(bool ignore)
        {
            this.Ignore = ignore;
        }
    }
    public class RepresentedModelValue
    {
        public RepresentedModelValue(FieldType VType)
        {
            this.VType = VType;
        }
        public FieldType VType { get; set; }
        public string Description { get; set; }
    }
}
