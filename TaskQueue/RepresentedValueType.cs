using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue
{
    public enum FieldType : byte
    {
        text = 0x11,
        num_int,
        num_long,
        num_double,
        boolean,
        datetime
    }

    public class RepresentedValueType
    {
        public FieldType Type { get; set; }
        public object Value { get; set; }
    }
}
