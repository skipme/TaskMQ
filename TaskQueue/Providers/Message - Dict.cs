using System;
using System.Collections.Generic;
using System.Dynamic;
//using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskQueue.Providers
{
    public abstract class TItemModel : ITItem
    {
        public Dictionary<string, object> Holder;
        //public System.Collections.Hashtable Holder;

        [FieldDescription(Ignore = true, Inherited = true, Required = false)]
        public abstract string ItemTypeName { get; set; }

        public TItemModel()
        {
        }
        public TItemModel(Dictionary<string, object> msgDict)
        //public TItemModel(System.Collections.Hashtable msgDict)
        {
            SetHolder(msgDict);
        }

        public RepresentedModel GetModel()
        {
            Type t = this.GetType();
            RepresentedModel model = new RepresentedModel(t);
            return model;
        }
        public void SetHolder(Dictionary<string, object> msgDict)
        {
            Type t = this.GetType();
            ValueMap<string, RepresentedModelValue> model = RepresentedModel.FindScheme(t);
            for (int idx = 0; idx < model.val1.Count; idx++)
            {
                string key = model.val1[idx];
                object val;
                if (msgDict.TryGetValue(key, out val))
                {
                    PropertyInfo pi = model.val2[idx].propertyDescriptor;
                    Type pt = pi.PropertyType;

                    bool nullable = false;
                    Type ptn = Nullable.GetUnderlyingType(pt);
                    if (nullable = ptn != null)
                        pt = ptn;

                    if (val == null && !nullable)
                    {
                        if (pt.BaseType != typeof(object))
                            throw new Exception("value does not accept nullable types, key: " + key);
                        else continue;
                    }
                    else if (!nullable && pt != val.GetType())
                        throw new Exception("unknown type for key: " + key);

                    //pi.SetValue(this, val, null);
                    model.val2[idx].PropValueSet(this, val);
                }
            }
            Holder = new Dictionary<string, object>(msgDict);
            //Holder = new System.Collections.Hashtable(msgDict);
        }
        //public void SetHolder(System.Collections.Hashtable msgDict)
        //{
        //    Type t = this.GetType();
        //    //ValueMap<string, RepresentedModelValue> model = new RepresentedModel(t).schema;
        //    ValueMap<string, RepresentedModelValue> model = RepresentedModel.FindScheme(t);
        //    //foreach (KeyValuePair<string, object> v in msgDict)
        //    //{
        //    //    int descIndex = model.IndexOf(v.Key);
        //    //    if (descIndex >= 0)
        //    //    {
        //    //        //PropertyInfo pi = t.GetProperty(v.Key);
        //    //        //if (pi == null)
        //    //        //    continue;
        //    //        PropertyInfo pi = model.val2[descIndex].propertyDescriptor;
        //    //        Type pt = pi.PropertyType;

        //    //        bool nullable = false;
        //    //        Type ptn = Nullable.GetUnderlyingType(pt);
        //    //        if (nullable = ptn != null)
        //    //            pt = ptn;

        //    //        if (v.Value == null && !nullable)
        //    //        {
        //    //            if (pt.BaseType != typeof(object))
        //    //                throw new Exception("value does not accept nullable types, key: " + v.Key);
        //    //            else continue;
        //    //        }
        //    //        else if (!nullable && pt != v.Value.GetType())
        //    //            throw new Exception("unknown type for key: " + v.Key);

        //    //        pi.SetValue(this, v.Value, null);
        //    //    }
        //    //}
        //    for (int idx = 0; idx < model.val1.Count; idx++)
        //    {
        //        string key = model.val1[idx];
        //        object val;
        //        //if (msgDict.TryGetValue(key, out val))
        //        if (msgDict.ContainsKey(key))
        //        {
        //            val = msgDict[key];
        //            //PropertyInfo pi = t.GetProperty(v.Key);
        //            //if (pi == null)
        //            //    continue;
        //            PropertyInfo pi = model.val2[idx].propertyDescriptor;
        //            Type pt = pi.PropertyType;

        //            bool nullable = false;
        //            Type ptn = Nullable.GetUnderlyingType(pt);
        //            if (nullable = ptn != null)
        //                pt = ptn;

        //            if (val == null && !nullable)
        //            {
        //                if (pt.BaseType != typeof(object))
        //                    throw new Exception("value does not accept nullable types, key: " + key);
        //                else continue;
        //            }
        //            else if (!nullable && pt != val.GetType())
        //                throw new Exception("unknown type for key: " + key);

        //            //pi.SetValue(this, val, null);
        //            model.val2[idx].PropValueSet(this, val);
        //        }
        //    }
        //    Holder = new System.Collections.Hashtable(msgDict);
        //}
        //void ReverseSetProps()
        //{
        //    if (Holder == null)
        //        return;
        //    Type t = this.GetType();
        //    ValueMap<string, RepresentedModelValue> model = RepresentedModel.FindScheme(t);
        //    foreach (KeyValuePair<string, object> v in Holder)
        //    {
        //        //PropertyInfo pi = t.GetProperty(v.Key);
        //        //if (pi == null)
        //        //    continue;
        //        int propDescIndex = model.IndexOf(v.Key);
        //        if (propDescIndex >= 0)
        //        {
        //            PropertyInfo pi = model.val2[propDescIndex].propertyDescriptor;
        //            pi.SetValue(this, v.Value, null);
        //        }
        //    }
        //}

        void SetProps()
        {
            if (Holder == null)
                //Holder = new System.Collections.Hashtable();
                Holder = new Dictionary<string, object>();

            Type t = this.GetType();
            //ValueMap<string, RepresentedModelValue> model = new RepresentedModel(t).schema;
            ValueMap<string, RepresentedModelValue> model = RepresentedModel.FindScheme(t);
            for (int i = 0; i < model.val1.Count; i++)
            {
                /*string k = model.val1[i];
                PropertyInfo pi = model.val2[i].propertyDescriptor; //t.GetProperty(k);
                object val = pi.GetValue(this, null);

                if (Holder.ContainsKey(k))
                    Holder[k] = val;
                else
                    Holder.Add(k, val);*/
                Holder[model.val1[i]] = model.val2[i].PropValue(this);
            }
        }

        //public dynamic ToExpando()
        //{
        //    SetProps();
        //    var result = new ExpandoObject();
        //    var d = result as IDictionary<string, object>; //work with the Expando as a Dictionary
        //    d = Holder;
        //    return result;
        //}
        //public Dictionary<string, object> ToDictionary()
        //{
        //    SetProps();
        //    return Holder;
        //}
        /// <summary>
        /// Rewrite internal dictionary values with fields available in model
        /// and return reference to it
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetHolder()
        //public System.Collections.Hashtable GetHolder()
        {
            SetProps();
            return Holder;
        }
        public ValueMap<string, object> GetValueMap(TQItemSelector order)
        {
            ValueMap<string, object> result = new ValueMap<string, object>();
            if (Holder == null) SetProps();
            foreach (KeyValuePair<string, TQItemSelectorParam> rule in order.parameters)
            {
                //object valobj;
                //if (Holder.TryGetValue(rule.Key, out valobj))
                //{
                //    result.Add(rule.Key, valobj);
                //}
                //else
                //{
                //    result.Add(rule.Key, null);
                //}
                result.Add(rule.Key, Holder[rule.Key]);
            }

            // extra data - without any order
            foreach (KeyValuePair<string, object> hdpair in Holder)
            //foreach (System.Collections.DictionaryEntry hdpair in Holder)
            {
                string k = (string)hdpair.Key;
                if (!order.parameters.ContainsKey(k))
                {
                    result.Add(k, hdpair.Value);
                }
            }
            return result;
        }
    }
}
