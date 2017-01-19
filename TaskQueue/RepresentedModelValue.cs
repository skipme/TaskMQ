using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskQueue
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldDescription : Attribute
    {
        public string Description;
        /// <summary>
        /// don't include to scheme (MType) -ignore for any validation and persistent
        /// </summary>
        public bool Ignore { get; set; }
        /// <summary>
        /// mark this field as required for module processing
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// property inherited from parent class -validation check only required field
        /// </summary>
        public bool Inherited { get; set; }

        public object DefaultValue { get; set; }

        public FieldDescription()
        { }
        public FieldDescription(string Description, bool Required = false)
        {
            this.Description = Description;
            this.Required = Required;
            this.Inherited = false;
        }

        public FieldDescription(bool ignore, bool inherited = false, bool required = false)
        {
            this.Ignore = ignore;
            this.Inherited = inherited;
            this.Required = required;
        }
    }

    public class RepresentedModelValue
    {
        public RepresentedModelValue(FieldType VType)
        {
            this.VType = VType;
        }
  
        public FieldType VType;
        public string Description;
        public bool Required;
        public bool Inherited;
        public object DefaultValue;

        public FieldType TypeOfField { get { return VType; } set { VType = value; } }
        public string FieldDescription { get { return Description; } set { Description = value; } }
        public bool FieldRequired { get { return Required; } set { Required = value; } }
        public bool FieldInherited { get { return Inherited; } set { Inherited = value; } }
        public object FieldDefaultValue { get { return DefaultValue; } set { DefaultValue = value; } }

        public PropertyInfo propertyDescriptor;
        public delegate object getProp(object instance);
        public getProp PropValue;
    }
}
