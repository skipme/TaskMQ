using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskQueue
{
    public class RepresentedModel
    {
        public ValueMap<string, RepresentedModelValue> schema = new ValueMap<string, RepresentedModelValue>();
        public static RepresentedModel Empty
        {
            get
            {
                return new RepresentedModel()
                {

                };
            }
        }
        private RepresentedModel() { }
        public RepresentedModel(Type classWithProps)
        {
            PropertyInfo[] props = classWithProps.GetProperties();

            foreach (PropertyInfo prop in props)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    bool isnull;
                    FieldType itv = GetRType(prop.PropertyType, out isnull);
                    RepresentedModelValue sch_v = new RepresentedModelValue(itv);

                    FieldDescription[] attrs = (FieldDescription[])prop.GetCustomAttributes(typeof(FieldDescription), false);
                    if (attrs.Length > 0)
                    {
                        sch_v.Description = attrs[0].Description;
                        if (attrs[0].Ignore)
                            continue;
                    }
                    schema.Add(prop.Name, sch_v);
                }
            }
        }

        public static FieldType GetRType(Type t, out bool nullable)
        {
            nullable = Nullable.GetUnderlyingType(t) != null;
            if (nullable)
                t = Nullable.GetUnderlyingType(t);
            if (t == typeof(int))
            {
                return FieldType.num_int;
            }
            if (t == typeof(double))
            {
                return FieldType.num_double;
            }
            if (t == typeof(string))
            {
                return FieldType.text;
            }
            if (t == typeof(DateTime))
            {
                return FieldType.datetime;
            }
            if (t == typeof(bool))
            {
                return FieldType.boolean;
            }
            return FieldType.text;
        }
    }
}
