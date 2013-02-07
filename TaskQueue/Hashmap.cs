using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue
{
    public class ValueMapItem<T, LT>
    {
        public T Value1 { get; set; }
        public LT Value2 { get; set; }
    }
    public class ValueMap<T, LT>
    {
        public List<T> val1;
        public List<LT> val2;
        public ValueMap() { val1 = new List<T>(); val2 = new List<LT>(); }

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

        public IEnumerable<ValueMapItem<T, LT>> ALL()
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
    }
}
