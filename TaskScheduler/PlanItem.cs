using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskUniversum.Task;

namespace TaskScheduler
{
    public class PlanItem
    {
        public string NameAndDescription;

        public IntervalType intervalType;
        public long intervalValue;
        public DateTime intervalTime;

        public PlanItemEntryPoint JobEntry;

        internal DateTime LastExecutionTime;
        internal DateTime NextExecutionTime;
        public void SetStartExecution()
        {
            ExucutingNow = true;
            LastExecutionTime = DateTime.UtcNow;

            switch (intervalType)
            {
                case IntervalType.intervalMilliseconds:
                    NextExecutionTime = LastExecutionTime.AddMilliseconds(intervalValue);
                    break;
                case IntervalType.intervalSeconds:
                    NextExecutionTime = LastExecutionTime.AddSeconds(intervalValue);
                    break;
                case IntervalType.DayTime:
                    if (LastExecutionTime.Date == DateTime.UtcNow.Date)// today already executed
                        NextExecutionTime = DateTime.Today.AddDays(1).AddHours(intervalTime.Hour).AddMinutes(intervalTime.Minute);
                    else
                        NextExecutionTime = DateTime.Today.AddHours(intervalTime.Hour).AddMinutes(intervalTime.Minute);
                    break;
                default:
                    NextExecutionTime = DateTime.UtcNow;
                    break;
            }
        }
        public volatile bool ExucutingNow;

        /// <summary>
        /// We can suspend this job for maintenance
        /// </summary>
        public bool Suspended;
        public double LAMS;

        public double MillisecondsBeforeExecute()
        {
            return LAMS = (NextExecutionTime - DateTime.UtcNow).TotalMilliseconds;
        }
    }
}
