using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskQueue
{
    public class RepresentedModel
    {
        public ValueMap<string, QueueItemModelValue> schema = new ValueMap<string, QueueItemModelValue>();
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
                    TItemValue_Type itv = GetRType(prop.PropertyType, out isnull);
                    QueueItemModelValue sch_v = new QueueItemModelValue(itv);

                    TQModelProp[] attrs = (TQModelProp[])prop.GetCustomAttributes(typeof(TQModelProp), false);
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

        public static TItemValue_Type GetRType(Type t, out bool nullable)
        {
            nullable = Nullable.GetUnderlyingType(t) != null;
            if (nullable)
                t = Nullable.GetUnderlyingType(t);
            if (t == typeof(int))
            {
                return TItemValue_Type.num_int;
            }
            if (t == typeof(double))
            {
                return TItemValue_Type.num_double;
            }
            if (t == typeof(string))
            {
                return TItemValue_Type.text;
            }
            if (t == typeof(DateTime))
            {
                return TItemValue_Type.datetime;
            }
            if (t == typeof(bool))
            {
                return TItemValue_Type.boolean;
            }
            return TItemValue_Type.text;
        }
    }
}
