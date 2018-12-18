using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum.Task
{
    public enum IntervalType
    {
        isolatedThread = 0x21,
        withoutInterval,
        
        //intervalMilliseconds,
        intervalSeconds,
        DayTime
    }
}
