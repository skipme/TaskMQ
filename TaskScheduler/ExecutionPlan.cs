using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskUniversum.Task;

namespace TaskScheduler
{
    public class ExecutionPlan
    {
        private List<PlanItem> PlanComponents = new List<PlanItem>();

        private PlanItem[] CurrentPlanQueue = new PlanItem[0];
        private int CPQueueCursor;

        public PlanItem Next()
        {
            PlanItem Dequeued;
            lock (CurrentPlanQueue)
            {
                bool planNotEmpty = CurrentPlanQueue.Length > CPQueueCursor;
                if (!planNotEmpty)
                {
                    PlanItem[] newcmpnts = OrderComponents();
                    planNotEmpty = newcmpnts.Length > 0;
                    if (!planNotEmpty)
                        return null;// plan is empty
                    else
                    {
                        // populate queue
                        CurrentPlanQueue = newcmpnts;
                        CPQueueCursor = 0;
                    }
                }

                Dequeued = CurrentPlanQueue[CPQueueCursor];
                CPQueueCursor++;
            }

            return Dequeued;
        }
        public void SetComponents(List<PlanItem> newComponents)
        {
            lock (PlanComponents)
            {
                lock (CurrentPlanQueue)
                {
                    PlanComponents = newComponents;
                    PopulateQueue(OrderComponents());
                }
            }

        }

        private PlanItem[] OrderComponents()
        {
            var p = from i in PlanComponents
                    where !i.Suspended && i.intervalType != IntervalType.isolatedThread &&
                    !i.ExucutingNow && i.MillisecondsBeforeExecute() <= 0
                    orderby i.LAMS
                    select i;
            return p.ToArray();
        }
        private void PopulateQueue(PlanItem[] p)
        {
            CurrentPlanQueue = p;
            CPQueueCursor = 0;
        }
    }
}
