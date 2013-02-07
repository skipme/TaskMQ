using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskScheduler
{
    public class PlanItem
    {
        public string NameAndDescription { get; set; }

        public IntervalType intervalType { get; set; }
        public long intervalValue { get; set; }
        public DateTime intervalTime { get; set; }

        public string ModuleName { get; set; }

        public PlanItemEntryPoint planEntry;

        public DateTime LastExecutionTime { get; set; }
        public bool ExucutingNow { get; set; }

        public object CustomObject { get; set; }

        public long MillisecondsBeforeExecute()
        {
            long ms = 0;
            switch (intervalType)
            {
                case IntervalType.withoutInterval:
                    ms = 0;
                    break;
                case IntervalType.everyCustomMilliseconds:
                    ms = (long)(LastExecutionTime.AddMilliseconds(intervalValue) - DateTime.Now).TotalMilliseconds;
                    break;
                case IntervalType.everyCustomSeconds:
                    ms = (long)(LastExecutionTime.AddSeconds(intervalValue) - DateTime.Now).TotalMilliseconds;
                    break;
                case IntervalType.everyDayAtTime:
                    if (LastExecutionTime.Date == DateTime.Now.Date)
                        ms = (long)(DateTime.Today.AddDays(1).AddHours(intervalTime.Hour).AddMinutes(intervalTime.Minute) - DateTime.Now).TotalMilliseconds;
                    else ms = (long)(DateTime.Today.AddHours(intervalTime.Hour).AddMinutes(intervalTime.Minute) - DateTime.Now).TotalMilliseconds;
                    break;
                case IntervalType.isolatedThread:

                    break;
                default:
                    break;
            }
            return ms;
        }
    }
}
