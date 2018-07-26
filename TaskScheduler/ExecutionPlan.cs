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

        private List<PlanItem> OnceJobs = new List<PlanItem>();// not prioritized
        /// <summary>
        /// Enqueue unprioritized backround job, maintenance job maybe
        /// </summary>
        /// <param name="job"></param>
        public void DoNonpiorityJob(PlanItem job)
        {
            lock (OnceJobs)
            {
                OnceJobs.Add(job);
            }
        }
        public PlanItem Next(bool wait)
        {
            PlanItem Dequeued = null;
            lock (CurrentPlanQueue)
            {
            	bool planNotEmpty = CurrentPlanQueue.Length > CPQueueCursor;
                if (planNotEmpty)
                {
                	Dequeued = CurrentPlanQueue[CPQueueCursor];
                    CPQueueCursor++;
                    Dequeued.SetStartExecution();
                }
                else
                {
                    PlanItem[] jnewcmpnts = null;
                    PlanItem[] newcmpnts = null;
                    lock (OnceJobs)
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
                    refilled.WaitOne(waitsec * 1000);
                }
                Dequeued = this.Next(false);
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
                     !i.ExucutingNow && i.SecondsBeforeExecute() <= 0
                     orderby i.LAMS descending
                     select i;
            return p.ToArray();    
        }
       
        private int BeforeNextSec()
        {
            lock (PlanComponents)
            {
                var n = from i in PlanComponents
                        where !i.Suspended && i.intervalType != IntervalType.isolatedThread &&
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
        private void PopulateQueue(PlanItem[] p)
        {
            CurrentPlanQueue = p;
            CPQueueCursor = 0;

            refilled.Set();
        }
    }
}
