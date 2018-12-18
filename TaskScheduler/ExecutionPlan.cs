using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TaskUniversum.Task;

namespace TaskScheduler
{
    public class ExecutionPlan
    {
        private Queue<PlanItem> DelayedTasks = new Queue<PlanItem>();
        private readonly object SyncDelayedTasks = new object();
        private volatile bool HasDelayedTasks;

        private List<PlanItem> ImmediateTasks;
        private int CounterIT;

        private List<PlanItem> ShortTermTasks;
        private int CounterST;

        private List<PlanItem> LongTermTasks;

        /// <summary>
        /// Enqueue unprioritized backround job, maintenance job maybe
        /// </summary>
        /// <param name="job"></param>
        public void DoNonpiorityJob(PlanItem job)
        {
            lock (SyncDelayedTasks)
            {
                DelayedTasks.Enqueue(job);
            }
            HasDelayedTasks = true;
        }
        public PlanItem Next(bool wait)
        {
            PlanItem resultedTask = null;

            int ctidx = Interlocked.Increment(ref CounterIT);
            if (ctidx > 0 && ctidx % ImmediateTasks.Count == 0)
            {
                if (ShortTermTasks.Count > 0)
                {
                    ctidx = -1;
                    CounterIT = -1;
                }
                else
                {
                    ctidx = 0;
                    CounterIT = 0;
                }
               
            }

            if (ctidx >= 0)
            {
                PlanItem cit = ImmediateTasks[ctidx % ImmediateTasks.Count];
                if (!cit.Suspended && !cit.ExucutingNow)
                {
                    cit.ExucutingNow = true;
                    resultedTask = cit;
                }
            }
            else
            {
                while (true)
                {
                    int idxst = Interlocked.Increment(ref CounterST);
                    if (idxst >= ShortTermTasks.Count)
                    {
                        CounterST = 0;
                        break;
                    }

                    PlanItem cit = ShortTermTasks[idxst];
                    if (!cit.Suspended && !cit.ExucutingNow && cit.SecondsBeforeExecute() <= 0)
                    {
                        cit.ExucutingNow = true;
                    }
                    else
                    {
                        resultedTask = cit;
                        break;
                    }
                }
            }
            if (resultedTask == null && HasDelayedTasks)
            {
                lock (SyncDelayedTasks)
                {
                    resultedTask = DelayedTasks.Dequeue();
                    if (DelayedTasks.Count == 0)
                    {
                        HasDelayedTasks = false;
                    }
                }
            }
            return resultedTask;
        }
        public void SetComponents(List<PlanItem> newComponents)
        {
            List<PlanItem> imm_t = new List<PlanItem>();
            List<PlanItem> shrt_cand_t = new List<PlanItem>();
            List<PlanItem> lngt_cand_t = new List<PlanItem>();
            for (int i = 0; i < newComponents.Count; i++)
            {
                PlanItem cit = newComponents[i];
                switch (cit.intervalType)
                {
                    case IntervalType.withoutInterval:
                        imm_t.Add(cit);
                        break;
                    case IntervalType.intervalSeconds:
                        shrt_cand_t.Add(cit);
                        break;
                    case IntervalType.DayTime:
                        lngt_cand_t.Add(cit);
                        break;
                    default:
                        break;
                }
            }

            ImmediateTasks = imm_t;
            ShortTermTasks = shrt_cand_t;
        }

        //private PlanItem[] OrderComponents()
        //{
        //    var p = from i in PlanComponents
        //            //where !i.Suspended && i.intervalType != IntervalType.isolatedThread &&
        //            //!i.ExucutingNow && i.SecondsBeforeExecute() <= 0
        //            where !i.Suspended && i.intervalType > IntervalType.withoutInterval &&
        //            !i.ExucutingNow && i.SecondsBeforeExecute() <= 0
        //            orderby i.LAMS descending
        //            select i;
        //    return p.ToArray();
        //}
        //private PlanItem[] OrderInstantComponents()
        //{
        //    var p = from i in PlanComponents
        //            where !i.Suspended && i.intervalType == IntervalType.withoutInterval
        //            // orderby i.LAMS descending // TODO: Add some priority
        //            select i;
        //    return p.ToArray();
        //}
        //private int BeforeNextSec()
        //{
        //    lock (planSync)
        //    {
        //        var n = from i in PlanComponents
        //                //where !i.Suspended && i.intervalType != IntervalType.isolatedThread &&
        //                //!i.ExucutingNow && i.SecondsBeforeExecute() > 0
        //                where !i.Suspended && i.intervalType > IntervalType.withoutInterval &&
        //                !i.ExucutingNow && i.SecondsBeforeExecute() > 0
        //                orderby i.LAMS
        //                select i;

        //        PlanItem NextJob = n.FirstOrDefault();
        //        if (NextJob == null)
        //            return 1;// default wait 1 sec
        //        else
        //        {
        //            return (int)NextJob.LAMS;
        //        }
        //    }
        //}
        //private void PopulateQueue(PlanItem[] Q)
        //{
        //    //lock (planSync)
        //    {
        //        CurrentPlanQueue = Q;
        //        CPQueueCursor = 0;

        //        refilled.Set();
        //    }
        //}

    }
}
