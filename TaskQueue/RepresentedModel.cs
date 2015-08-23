using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace TaskQueue
{
    public class RepresentedModel
    {
        internal static Dictionary<Type, RepresentedModel> SchemeCache = new Dictionary<Type, RepresentedModel>();

        public static ValueMap<string, RepresentedModelValue> FindScheme(Type classWithProps)
        {
            RepresentedModel lookupModel;
            lock (SchemeCache)
                if (!SchemeCache.TryGetValue(classWithProps, out lookupModel))
                {
                    lookupModel = new RepresentedModel(classWithProps);
                    SchemeCache.Add(classWithProps, lookupModel);
                }

            return lookupModel.schema;
        }

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
                        sch_v.propertyDescriptor = prop;
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
            else if (t == typeof(long))
            {
                return FieldType.num_long;
            }
            else if (t == typeof(double))
            {
                return FieldType.num_double;
            }
            else if (t == typeof(string))
            {
                return FieldType.text;
            }
            else if (t == typeof(DateTime))
            {
                return FieldType.datetime;
            }
            else if (t == typeof(bool))
            {
                return FieldType.boolean;
            }
            return FieldType.text;
        }
        static readonly Dictionary<FieldType, string> ftMapSimlinkType = new Dictionary<FieldType, string>()
         {
             {FieldType.text,       "string"},
             {FieldType.num_int,    "int"},
             {FieldType.num_long,   "long"},
             {FieldType.num_double, "double"},
             {FieldType.boolean,    "bool"},
             {FieldType.datetime,   "DateTime"}
         };
        public static string GetLTypeString(FieldType ft)
        {
            string result;
            if (ftMapSimlinkType.TryGetValue(ft, out result))
                return result;
            return null;
        }
        public string CalculateSchemeHash()
        {
            List<byte> b = new List<byte>();
            string result = "";

            for (int i = 0; i < schema.val1.Count; i++)
            {
                RepresentedModelValue rv = schema.val2[i];
                if (rv.Required)
                {
                    b.AddRange(Encoding.UTF8.GetBytes(schema.val1[i]));

                    //b.Add((byte)(rv.Required ? 1 : 0));
                    b.Add((byte)(rv.VType));
                }
            }
            byte[] hash = (new System.Security.Cryptography.SHA1Managed()).ComputeHash(b.ToArray());
            for (int i = 0; i < hash.Length; i++)
            {
                result += hash[i].ToString("x2");
            }
            return result;

        }
    }
}
