using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Statistics
{
    public class StatHub
    {
        public static int[] useRanges = new int[] { StatRange.seconds30, StatRange.min, StatRange.min30, StatRange.hour2 };
       
        public T FindModel<T>()
            where T : StatMatchModel
        {
            return null;
        }
    }
}
