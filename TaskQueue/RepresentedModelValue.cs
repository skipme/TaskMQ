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
        /// <summary>
        /// don't include to scheme (MType)
        /// </summary>
        public bool Ignore { get; set; }
        public bool Required { get; set; }

        public FieldDescription(string Description, bool Required = false)
        {
            this.Description = Description;
            this.Required = Required;
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
        public bool Required { get; set; }
    }
}
