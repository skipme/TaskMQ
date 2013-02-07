using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue
{
    public enum TItemValue_Type
    {
        text = 0x11,
        num_int,
        num_double,
        boolean,
        datetime
    }

    public class TItemValue
    {
        public TItemValue_Type Type { get; set; }
        public object Value { get; set; }
    }
}
