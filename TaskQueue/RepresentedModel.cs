using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskQueue
{
    public class RepresentedModel
    {
        public ValueMap<string, RepresentedModelValue> schema;
        public static RepresentedModel FromSchema(Dictionary<string, RepresentedModelValue> schema)
        {
            return new RepresentedModel
            {
                schema = new ValueMap<string, RepresentedModelValue>(schema)
            };
        }
        public static RepresentedModel Empty
        {
            get
            {
                return new RepresentedModel()
                {
                    schema = new ValueMap<string, RepresentedModelValue>()
                };
            }
        }
        public Dictionary<string, string> ToDeclareDictionary()
        {
            return schema.ToList().ToDictionary((keyItem) => keyItem.Value1, (valueItem) => valueItem.Value2.VType.ToString());
        }
        private RepresentedModel() { }
        public RepresentedModel(Type classWithProps)
        {
            schema = new ValueMap<string, RepresentedModelValue>();
            PropertyInfo[] props = classWithProps.GetProperties();

            foreach (PropertyInfo prop in props)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    bool isnull;
                    FieldType itv = GetRType(prop.PropertyType, out isnull);
                    RepresentedModelValue sch_v = new RepresentedModelValue(itv);

                    FieldDescription[] attrs = (FieldDescription[])prop.GetCustomAttributes(typeof(FieldDescription), true);
                    if (attrs.Length > 0)
                    {
                        sch_v.Description = attrs[0].Description;
                        sch_v.Required = attrs[0].Required;
                        sch_v.Inherited = attrs[0].Inherited;
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
        public static string GetLTypeString(FieldType ft)
        {
            switch (ft)
            {
                case FieldType.text:
                    return "string";
                case FieldType.num_int:
                    return "int";
                case FieldType.num_double:
                    return "double";
                case FieldType.boolean:
                    return "bool";
                case FieldType.datetime:
                    return "DateTime";
                default:
                    break;
            }
            return "string";
        }
    }
}
