﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue
{
    public enum TQItemSelectorSet
    {
        Ascending = 0x11,
        Descending = 0x12,
        Equals = 0x21,
        NotEquals = 0x22
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
            this.Rule(key, set);
        }

        public TQItemSelector(string key, object value, TQItemSelectorSet set = TQItemSelectorSet.Equals)
        {
            this.Rule(key, value, set);
        }
        /// <summary>
        /// SORT messages by field values and set manner(asc or desc)
        /// </summary>
        /// <param name="key">field name</param>
        /// <param name="set">sort manner</param>
        /// <returns></returns>
        public TQItemSelector Rule(string key, TQItemSelectorSet set)
        {
            if (set == TQItemSelectorSet.Equals
                || set == TQItemSelectorSet.NotEquals)
                throw new Exception("This Rule build function for sort manner only");

            this.parameters.Add(key,
                new TQItemSelectorParam()
                {
                    Key = key,
                    Value = null,
                    ValueSet = set
                });
            return this;
        }
        /// <summary>
        /// IF field in message equals with object
        /// </summary>
        /// <param name="key">field name</param>
        /// <param name="value">value for equality test</param>
        /// <returns></returns>
        public TQItemSelector Rule(string key, object value, TQItemSelectorSet set = TQItemSelectorSet.Equals)
        {
            if (set == TQItemSelectorSet.Ascending
                || set == TQItemSelectorSet.Descending)
                throw new Exception("This Rule build function for comparing manner only");

            this.parameters.Add(key,
                new TQItemSelectorParam()
                {
                    Key = key,
                    Value = value,
                    ValueSet = set
                });
            return this;
        }
    }
}
