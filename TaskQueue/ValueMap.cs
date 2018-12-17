using System;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace TaskQueue
{
    public class ValueMapItem<T, LT>
    {
        public T Value1;
        public LT Value2;
    }
    public class ValueMap<T, LT>
    {
        public List<T> val1;
        public List<LT> val2;
        public ValueMap()
        {
            val1 = new List<T>();
            val2 = new List<LT>();
        }
        public ValueMap(Dictionary<T, LT> bdict)
        {
            val1 = new List<T>();
            val2 = new List<LT>();
            foreach (KeyValuePair<T, LT> bdictItem in bdict)
            {
                val1.Add(bdictItem.Key);
                val2.Add(bdictItem.Value);
            }
        }
        public void Add(T a, LT b)
        {
            val1.Add(a);
            val2.Add(b);
        }
        public void Clear()
        {
            val1.Clear();
            val2.Clear();
        }
        //public int IndexOf(T key)
        //{
        //    return val1.IndexOf(key);
        //}
        public List<ValueMapItem<T, LT>> ToList()
        {
            List<ValueMapItem<T, LT>> result = new List<ValueMapItem<T, LT>>();
            for (int i = 0; i < val1.Count; i++)
            {
                result.Add(new ValueMapItem<T, LT>
                {
                    Value1 = val1[i],
                    Value2 = val2[i]
                });
            }
            return result;
        }
        public Dictionary<T, LT> ToDictionary()
        {
            Dictionary<T, LT> result = new Dictionary<T, LT>();
            for (int i = 0; i < val1.Count; i++)
            {
                result.Add(val1[i], val2[i]);
            }
            return result;
        }
        public Hashtable ToHashtable()
        {
            Hashtable result = new Hashtable();
            for (int i = 0; i < val1.Count; i++)
            {
                result.Add(val1[i], val2[i]);
            }
            return result;
        }
    }
}
