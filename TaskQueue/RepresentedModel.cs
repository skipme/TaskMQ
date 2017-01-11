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
        //public Dictionary<string, string> ToDeclareDictionary()
        //{
        //    return schema.ToList().ToDictionary((keyItem) => keyItem.Value1, (valueItem) => valueItem.Value2.VType.ToString());
        //}
        private RepresentedModel() { }
        public RepresentedModel(Type classWithProps)
        {
            schema = new ValueMap<string, RepresentedModelValue>();
            PropertyInfo[] props = classWithProps.GetProperties();

            foreach (PropertyInfo prop in props)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    FieldDescription[] attrs = (FieldDescription[])prop.GetCustomAttributes(typeof(FieldDescription), true);
                    bool isnull;
                    FieldType itv = GetRType(prop.PropertyType, out isnull);
                    RepresentedModelValue sch_v = new RepresentedModelValue(itv)
                    {
                        propertyDescriptor = prop
                    };
                    if (attrs.Length > 0)
                    {
                        FieldDescription at = attrs[0];
                        if (!at.Ignore)
                        {
                            sch_v.Description = at.Description;
                            sch_v.Required = at.Required;
                            sch_v.Inherited = at.Inherited;

                            schema.Add(prop.Name, sch_v);
                        }
                    }
                    else
                    {
                        schema.Add(prop.Name, sch_v);
                    }
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
        /// <summary>
        /// Compatibility hash
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// For Vector hash
        /// </summary>
        /// <returns></returns>
        public byte[] LightweightSchemeProjection()
        {
            List<byte> b = new List<byte>();

            for (int i = 0; i < schema.val1.Count; i++)
            {
                RepresentedModelValue rv = schema.val2[i];
                //if (!Ignored) ignored not in list anyway
                //{
                b.AddRange(Encoding.UTF8.GetBytes(schema.val1[i]));
                b.Add((byte)(rv.VType));
                //}
            }
 
            return b.ToArray();
        }
        public static object Convert(object src, Type toType)
        {
            Type srcType = src.GetType();
            if (srcType == toType)
                return src;
            //object returnObj = null;
            bool nullable;
            FieldType FT = RepresentedModel.GetRType(srcType, out nullable);
            FieldType TT = RepresentedModel.GetRType(toType, out nullable);
            switch (TT)
            {
                case FieldType.text:
                    return src.ToString();
                case FieldType.num_int:
                    switch (FT)
                    {
                        case FieldType.text:
                            int tmpint;
                            int.TryParse(src as string, out tmpint);
                            return tmpint;
                        case FieldType.num_long:
                            unchecked
                            {
                                return (int)(/*(long)*/src);
                            }
                        case FieldType.num_double:
                            unchecked
                            {
                                return (int)(/*(long)*/src);
                            }
                        case FieldType.boolean:
                            return ((bool)src) ? 1 : 0;
                        //case FieldType.datetime:
                        //    break;
                        default:
                            return null;
                    }
                //break;
                case FieldType.num_long:
                    switch (FT)
                    {
                        case FieldType.text:
                            long tmp;
                            long.TryParse(src as string, out tmp);
                            return tmp;
                        case FieldType.num_int:
                            unchecked
                            {
                                return (long)((int)src);
                            }
                        case FieldType.num_double:
                            unchecked
                            {
                                return (long)((double)src);
                            }
                        case FieldType.boolean:
                            return ((bool)src) ? 1L : 0L;
                        case FieldType.datetime:
                            return ((DateTime)src).Ticks;
                            break;
                        default:
                            return null;
                    }
                //break;
                case FieldType.num_double:
                    switch (FT)
                    {
                        case FieldType.text:
                            double tmp;
                            double.TryParse(src as string, System.Globalization.NumberStyles.None,
                                System.Globalization.CultureInfo.InvariantCulture, out tmp);
                            return tmp;
                        case FieldType.num_long:
                            unchecked
                            {
                                return (double)(/*(long)*/src);
                            }
                        case FieldType.num_int:
                            unchecked
                            {
                                return (int)(/*(long)*/src);
                            }
                        case FieldType.boolean:
                            return ((bool)src) ? 1.0 : 0.0;
                        default:
                            return null;
                    }
                //break;
                case FieldType.boolean:
                    switch (FT)
                    {
                        case FieldType.text:
                            return !string.IsNullOrWhiteSpace(src as string);
                        case FieldType.num_long:
                            return ((long)src) != 0L;
                        case FieldType.num_int:
                            return ((int)src) != 0;
                        case FieldType.num_double:
                            return ((double)src) != 0.0;
                        default:
                            return null;
                    }
                //break;
                case FieldType.datetime:
                    switch (FT)
                    {
                        case FieldType.text:
                            DateTime tmpint;
                            DateTime.TryParse(src as string, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tmpint);
                            return tmpint;
                        case FieldType.num_long:
                            return DateTime.FromBinary((long)src);
                        default:
                            return null;
                    }
                //break;
                default:
                    return null;
            }
        }
    }
}
