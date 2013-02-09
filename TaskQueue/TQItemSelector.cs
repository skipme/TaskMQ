using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue
{
    public enum TQItemSelectorSet
    {
        Ascending = 0x11,
        Descending = 0x12,
        Equals = 0x21
    }
    public class TQItemSelectorParam
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public TQItemSelectorSet ValueSet { get; set; }
    }
    public class TQItemSelector
    {
        public Dictionary<string, TQItemSelectorParam> parameters = new Dictionary<string, TQItemSelectorParam>();

        public static TQItemSelector DefaultFifoSelector
        {
            get
            {
                TQItemSelector selector = new TQItemSelector("Processed", false)
                    .Rule("AddedTime", TQItemSelectorSet.Ascending);
                return selector;
            }
        }

        public TQItemSelector(string key, TQItemSelectorSet set)
        {
            if (set != TQItemSelectorSet.Ascending || set != TQItemSelectorSet.Descending)
                throw new Exception();
            this.Rule(key, set);
        }

        public TQItemSelector(string key, object value)
        {
            this.Rule(key, value);
        }
        public TQItemSelector Rule(string key, TQItemSelectorSet set)
        {
            this.parameters.Add(key,
                new TQItemSelectorParam()
                {
                    Key = key,
                    Value = null,
                    ValueSet = set
                });
            return this;
        }

        public TQItemSelector Rule(string key, object value)
        {
            this.parameters.Add(key,
                new TQItemSelectorParam()
                {
                    Key = key,
                    Value = value,
                    ValueSet = TQItemSelectorSet.Equals
                });
            return this;
        }
    }
}
