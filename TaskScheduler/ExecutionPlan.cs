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
        private readonly AutoResetEvent refilled = new AutoResetEvent(false);
        private List<PlanItem> PlanComponents = new List<PlanItem>();

        private PlanItem[] CurrentPlanQueue = new PlanItem[0];
        private int CPQueueCursor;

        private readonly object instantPlanQueueSync = new object();
        private PlanItem[] InstantPlanQueue = new PlanItem[0];
        private int InstantPQueueCursor;

        private readonly object onceJobsSync = new object();
        private List<PlanItem> OnceJobs = new List<PlanItem>();// not prioritized

        private readonly object planSync = new object();

        /// <summary>
        /// Enqueue unprioritized backround job, maintenance job maybe
        /// </summary>
        /// <param name="job"></param>
        public void DoNonpiorityJob(PlanItem job)
        {
            lock (onceJobsSync)
            {
                OnceJobs.Add(job);
            }
        }
        public PlanItem Next(bool wait)
        {
            PlanItem Dequeued = null;
            lock (instantPlanQueueSync)
            {
                if (InstantPlanQueue.Length > InstantPQueueCursor)
                {
                    Dequeued = InstantPlanQueue[InstantPQueueCursor];
                    if (!Dequeued.ExucutingNow)
                    {
                        InstantPQueueCursor++;
                        //Dequeued.SetStartExecution();
                        Dequeued.ExucutingNow = true;
                        return Dequeued;
                    }
                }
                //else 
                if (InstantPlanQueue.Length > 0)
                {
                    bool next = false;
                    //lock (planSync)
                    //{
                    next = CurrentPlanQueue.Length > CPQueueCursor;
                    //}
                    if (!next)
                    {
                        while (true)
                        {
                            if (InstantPQueueCursor >= InstantPlanQueue.Length)
                            {
                                InstantPQueueCursor = 0;
                                //Thread.Sleep(0);
                                Thread.Yield();
                            }
                            Dequeued = InstantPlanQueue[InstantPQueueCursor];
                            InstantPQueueCursor++;

                            if (Dequeued.ExucutingNow)
                                continue;
                            //Dequeued.SetStartExecution();
                            Dequeued.ExucutingNow = true;
                            break;
                        }
                        return Dequeued;
                    }
                    InstantPQueueCursor = 0;
                }
            }
            lock (planSync)
            {
                bool planNotEmpty = CurrentPlanQueue.Length > CPQueueCursor;
                //int qq = CPQueueCursor;
                if (planNotEmpty)
                {
                    //if (CPQueueCursor != qq) throw new Exception();
                    Dequeued = CurrentPlanQueue[CPQueueCursor];
                    CPQueueCursor++;
                    Dequeued.SetStartExecution();
                }
                else
                {
                    PlanItem[] jnewcmpnts = null;
                    PlanItem[] newcmpnts = null;
                    lock (onceJobsSync)
                    {
                        if (OnceJobs.Count > 0)
                        {
                            jnewcmpnts = OnceJobs.ToArray();
                            OnceJobs.Clear();
                        }
                    }
                    if (jnewcmpnts != null)// deferred jobs found, refill with plan
                    {
                        PlanItem[] Pnewcmpnts = OrderComponents();

                        newcmpnts = new PlanItem[jnewcmpnts.Length + Pnewcmpnts.Length];
                        Array.Copy(jnewcmpnts, newcmpnts, jnewcmpnts.Length);
                        Array.Copy(Pnewcmpnts, 0, newcmpnts, jnewcmpnts.Length, Pnewcmpnts.Length);
                    }
                    else// deferred jobs not found, try to refill plan
                    {
                        newcmpnts = OrderComponents();
                    }
                    planNotEmpty = newcmpnts.Length > 0;
                    if (planNotEmpty)
                    {
                        if (newcmpnts.Length > 1)
                        {
                            // populate queue
                            PopulateQueue(newcmpnts);// let wait handled thread do that with race for job **
                            // exit from losck 
                            //**
                            Dequeued = CurrentPlanQueue[CPQueueCursor];
                            CPQueueCursor++;// this thread has exclusive access without race for job
                            Dequeued.SetStartExecution();
                        }
                        else
                        {
                            Dequeued = newcmpnts[0];// this thread has exclusive access without race for job
                            Dequeued.SetStartExecution();
                        }
                    }
                    else
                    {
                        Dequeued = null;// plan is empty
                    }

                }
            }
            //if (Dequeued != null)
            //{
            //    lock (PlanComponents)
            //    {
            //        Dequeued.SetStartExecution();
            //    }
            //}
            //else if (wait)

            if (wait && Dequeued == null)
            {
                // check if we have a job to wait
                int waitsec = this.BeforeNextSec();
                //Console.WriteLine("waitms: {0}", waitms);
                if (waitsec > 0)
                {
                    Console.WriteLine("wait {0}", Thread.CurrentThread.Name);
                    refilled.WaitOne(waitsec * 1000);// - System.DateTime.UtcNow.Millisecond);
                }
                Dequeued = this.Next(false);
            }

            //if (Dequeued == null && InstantPlanQueue.Length > 0)
            //{
            //    Dequeued = InstantPlanQueue[InstantPQueueCursor];
            //    InstantPQueueCursor++;
            //    //Dequeued.SetStartExecution();
            //}
            //Console.WriteLine("deq {0} {1}", Dequeued == null, InstantPlanQueue.Length);
            return Dequeued;
        }
        public void SetComponents(List<PlanItem> newComponents)
        {
            lock (planSync)
            {
                PlanComponents = newComponents;
                PopulateQueue(OrderComponents(), OrderInstantComponents());
            }
        }

        private PlanItem[] OrderComponents()
        {
            var p = from i in PlanComponents
                    //where !i.Suspended && i.intervalType != IntervalType.isolatedThread &&
                    //!i.ExucutingNow && i.SecondsBeforeExecute() <= 0
                    where !i.Suspended && i.intervalType > IntervalType.withoutInterval &&
                    !i.ExucutingNow && i.SecondsBeforeExecute() <= 0
                    orderby i.LAMS descending
                    select i;
            return p.ToArray();
        }
        private PlanItem[] OrderInstantComponents()
        {
            var p = from i in PlanComponents
                    where !i.Suspended && i.intervalType == IntervalType.withoutInterval
                    // orderby i.LAMS descending // TODO: Add some priority
                    select i;
            return p.ToArray();
        }
        private int BeforeNextSec()
        {
            lock (planSync)
            {
                var n = from i in PlanComponents
                        //where !i.Suspended && i.intervalType != IntervalType.isolatedThread &&
                        //!i.ExucutingNow && i.SecondsBeforeExecute() > 0
                        where !i.Suspended && i.intervalType > IntervalType.withoutInterval &&
                        !i.ExucutingNow && i.SecondsBeforeExecute() > 0
                        orderby i.LAMS
                        select i;

                PlanItem NextJob = n.FirstOrDefault();
                if (NextJob == null)
                    return 1;// default wait 1 sec
                else
                {
                    return (int)NextJob.LAMS;
                }
            }
        }
        private void PopulateQueue(PlanItem[] Q)
        {
            //lock (planSync)
            {
                CurrentPlanQueue = Q;
                CPQueueCursor = 0;

                refilled.Set();
            }
        }
        private void PopulateQueue(PlanItem[] Q, PlanItem[] instantQ)
        {
            //lock (planSync)
            {
                lock (instantPlanQueueSync)
                {
                    CurrentPlanQueue = Q;
                    InstantPlanQueue = instantQ;

                    CPQueueCursor = InstantPQueueCursor = 0;
                }
                refilled.Set();
            }
        }
    }
}
