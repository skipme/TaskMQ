using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue
{
    /// <summary>
    /// Please note, 
    /// every type must have his own distinct not changed number
    /// </summary>
    public enum FieldType : byte
    {
        text        = 10,
        num_int     = 11,
        num_long    = 12,
        num_double  = 13,
        num_decimal = 14,
        boolean     = 15,
        datetime    = 16
    }

    public class RepresentedValueType
    {
        public FieldType Type { get; set; }
        public object Value { get; set; }
    }
}
