using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private PlanItem[] ImmediateTasks;
        private int ImmediateTasksCount;
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
        private int throttle_cycles = 0;
        private int ticks_throttle_cycle;
        private long throttle_cycle_ticks;
        public PlanItem Next()
        {
            PlanItem resultedTask = null;
            int ctidx;

            while (true)
            {
                ctidx = Interlocked.Increment(ref CounterIT);

                if (ctidx % ImmediateTasksCount == 0 && ctidx != 0)
                {
                    //int throttle_c_idx = Interlocked.Increment(ref throttle_cycles);
                    
                    //if (throttle_c_idx >= 10)
                    //{
                        //throttle_cycles = 0;
                        long timer = Stopwatch.GetTimestamp();
                        if ((timer - throttle_cycle_ticks) > ticks_throttle_cycle)
                        {
                            throttle_cycle_ticks = timer;
                            ctidx = -1;
                            CounterIT = -1;
                            break;
                        }
                    //}
                    else
                    {
                        ctidx = 0;
                        CounterIT = 0;
                    }
                }

                PlanItem cit = ImmediateTasks[ctidx % ImmediateTasksCount];
                if (!cit.ExucutingNow && !cit.Suspended)
                {
                    cit.ExucutingNow = true;// right now, tasks without delays can be executed in parallel
                    resultedTask = cit;
                    break;
                }

            }

            //int ctidx = Interlocked.Increment(ref CounterIT);
            //if (ctidx > 0 && ctidx % ImmediateTasks.Count == 0)
            //{
            //    //int throttle_c_idx = Interlocked.Increment(ref throttle_cycles);
            //    //if (throttle_c_idx >= 4)
            //    //{
            //    //    throttle_cycles = 0;
            //    ctidx = -1;
            //    CounterIT = -1;
            //    //}
            //    //else
            //    //{
            //    //    ctidx = 0;
            //    //    CounterIT = 0;
            //    //}
            //}

            //if (ctidx >= 0)
            //{
            //    PlanItem cit = ImmediateTasks[ctidx % ImmediateTasks.Count];
            //    if (!cit.Suspended && !cit.ExucutingNow)
            //    {
            //        cit.ExucutingNow = true;
            //        resultedTask = cit;
            //    }
            //}
            //else
            if (ctidx < 0)
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
                    lock (cit)
                    {
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
            ImmediateTasks = imm_t.ToArray();
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
            // --- 
            ticks_throttle_cycle = (int)(Stopwatch.Frequency * 0.3/*750ms*/);
            ImmediateTasksCount = ImmediateTasks.Length;
        }
    }
}
