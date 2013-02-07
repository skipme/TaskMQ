using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskScheduler
{
    public enum IntervalType
    {
        withoutInterval = 0x11,
        
        everyCustomMilliseconds,
        everyCustomSeconds,
        everyDayAtTime,

        isolatedThread
    }
}
