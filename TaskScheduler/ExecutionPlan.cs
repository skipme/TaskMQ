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
        private readonly object ShortTermLongTermTransferSync = new object();
        private int CounterST;

        private List<PlanItem> LongTermTasks;

        public static readonly int LongTermBarrierSec = 120;

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
                    PlanItem cit = null;
                    int idxst = -1;
                    lock (ShortTermLongTermTransferSync)
                    {
                        CounterST++;
                        idxst = CounterST;
                        if (idxst >= ShortTermTasks.Count)
                        {
                            CounterST = -1;
                            break;
                        }
                        cit = ShortTermTasks[idxst];
                    }

                    if (!cit.Suspended && !cit.ExucutingNow)
                    {
                        double sec_wait = cit.SecondsBeforeExecute();
                        if (sec_wait < 0)
                        {
                            cit.SetStartExecution();
                            resultedTask = cit;
                            break;
                        }
                        else if (sec_wait > LongTermBarrierSec)// 2 min
                        {
                            lock (ShortTermLongTermTransferSync)
                            {
                                ShortTermTasks.RemoveAt(idxst);
                                LongTermTasks.Add(cit);
                            }
                        }
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
            LongTermTasks = lngt_cand_t;

            ShortTermTasks.Add(new PlanItem
            {
                intervalType = IntervalType.intervalSeconds,
                intervalValue = 75,
                JobEntry = (ThreadContext ti, PlanItem pi) =>
                {
                    for (int i = 0; i < LongTermTasks.Count; i++)
                    {
                        PlanItem lti = LongTermTasks[i];
                        if (!lti.Suspended && lti.SecondsBeforeExecute() < LongTermBarrierSec)
                        {
                            lock (ShortTermLongTermTransferSync)
                            {
                                LongTermTasks.RemoveAt(i);
                                ShortTermTasks.Add(lti);
                            }
                            i--;   
                        }
                    }
                    return 1;
                }
            });
        }
    }
}
