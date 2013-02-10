using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskScheduler
{
    public class ExecutionPlan
    {
        public List<PlanItem> PlanComponents = new List<PlanItem>();
        public List<PlanItem> CurrentPlanQueue = null;
        public int QueueCursor { get; set; }
 
        public PlanItem Next()
        {
            if (CurrentPlanQueue == null || CurrentPlanQueue.Count == 0)
            {
                Create();
            }
            PlanItem pi = null;
            //if (CurrentPlanQueue.Count == 0)
            //{
            //    Create();
            //    pi = SubNext();
            //    if (pi == null)
            //    {
            //        return null;
            //    }
            //}

            pi = SubNext();
            if (pi == null)
            {
                Create();
                pi = SubNext();
            }
            return pi;
        }

        private PlanItem SubNext()
        {
            PlanItem pi = null;
            for (int i = QueueCursor; i < CurrentPlanQueue.Count; i++)
            {
                if (!CurrentPlanQueue[i].ExucutingNow)
                {
                    QueueCursor = i + 1;
                    pi = CurrentPlanQueue[i];
                    break;
                }
            }
            return pi;
        }
        public List<PlanItem> Create()
        {
            var p = from i in PlanComponents
                    where i.intervalType != IntervalType.isolatedThread &&
                    !i.ExucutingNow && i.MillisecondsBeforeExecute() <= 0
                    orderby i.MillisecondsBeforeExecute()
                    select i;
            QueueCursor = 0;
            return CurrentPlanQueue = p.ToList();
        }
    }
}
