using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum
{
    public enum IntervalType
    {
        withoutInterval = 0x11,
        
        intervalMilliseconds,
        intervalSeconds,
        DayTime,

        isolatedThread
    }
}
