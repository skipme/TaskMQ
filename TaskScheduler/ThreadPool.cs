using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TaskUniversum.Task;

namespace TaskScheduler
{
    public class ThreadContext
    {
        public bool Isolated { get; set; }
        public Thread hThread { get; set; }

        public bool StopThread { get; set; }
        public bool StoppedThread { get; set; }

        public int ManagedID { get; set; }

        public PlanItem Job;
        public ExecutionPlan rootPlan;
    }

    public class ThreadPool
    {
        const int minThrads = 4;
        const int maxThreads = 4;// TODO: replace it with some max value, if jobs executed too long allocate new threads according to this value

        List<ThreadContext> threads = new List<ThreadContext>();
        private ExecutionPlan plan = new ExecutionPlan();
        public ThreadPool()
        {
            this.ProcessorCount = Environment.ProcessorCount;
            Revoke();
        }
        // Throughput tune
        public int ProcessorCount { get; set; }
        public void IncrementWorkingThreads()
        {

        }
        public void DecrementWorkingThreads()
        {

        }
        // ~Throughput tune
        public bool Activity
        {
            get
            {
                foreach (ThreadContext t in threads)
                {
                    /* IsAlive checking if appdomain unloaded(forced stopping)*/
                    if (t.hThread.IsAlive &&
                        !t.StoppedThread)
                        return true;
                }
                return false;
            }
        }
        //public void Allocate()
        public void Revoke()
        {
            if (threads.Count > 0)
            {
                for (int i = 0; i < threads.Count; i++)
                {
                    if (threads[i].StoppedThread)
                    {
                        threads[i].StopThread = false;
                        threads[i].StoppedThread = false;

                        if (!threads[i].hThread.IsAlive)
                            threads.RemoveAt(i--);
                    }
                }
            }

            for (int i = 0; threads.Count < maxThreads; i++)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(ThreadEntry));
                thread.Name = "TaskScheduler Thread#" + i.ToString();
                ThreadContext ti = new ThreadContext()
                {
                    hThread = thread,
                    rootPlan = plan
                };

                thread.Start(ti);
                threads.Add(ti);
            }
        }
        public void SetPlan(List<PlanItem> setPlanItems)
        {
            plan.SetComponents(setPlanItems);
        }
        public void SetPlan(params PlanItem[] args)
        {
            plan.SetComponents(args.ToList());
        }
        public void SetPlan(IEnumerable<PlanItem> planItems)
        {
            plan.SetComponents(planItems.ToList());
        }
        public void DeferJob(PlanItemEntryPoint job)
        {
            this.DeferJob(new PlanItem
            {
                JobEntry = job,
                NameAndDescription = "deffered job"
            });
        }
        public void DeferJob(PlanItem job)
        {
            plan.DoNonpiorityJob(job);
        }
        public void CloseIsolatedThreads()
        {
            foreach (ThreadContext t in threads)
            {
                if (t.Isolated)
                {
                    if (t.hThread.IsAlive)
                    {
                        t.hThread.Abort();
                    }
                }
            }
        }
        public void CreateIsolatedThreadForPlan(PlanItem pi)
        {
            if (pi.intervalType == IntervalType.isolatedThread)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(IsolatedThreadEntry));
                thread.Name = "TaskScheduler IsolatedThread#" + pi.NameAndDescription;
                ThreadContext ti = new ThreadContext()
                {
                    hThread = thread,
                    rootPlan = plan,
                    Job = pi,
                    Isolated = true
                };

                thread.Start(ti);
                threads.Add(ti);
            }
        }
        public void SuspendAll()
        {
            for (int i = 0; i < threads.Count; i++)
            {
                threads[i].StopThread = true;
            }
        }
        /// <summary>
        /// Pull next job from execution plan
        /// </summary>
        /// <param name="ti"></param>
        static void IntermediateThread(ThreadContext ti)
        {
            PlanItem nextJob;

            if (ti.Job != null)
            {
                ti.Job.ExucutingNow = false;
            }

            nextJob = ti.rootPlan.Next(true);

            ti.Job = nextJob;
        }
        static void ExitThread(ThreadContext ti)
        {
            ti.StoppedThread = true;
            //Console.WriteLine("Exit" + ti.ManagedID);
        }
        /// <summary>
        /// if the Queue is empty we can suspend this thread for non blocking other jobs
        /// </summary>
        const int maxSuspend = 100;
        /// <summary>
        /// All threads do it ;
        /// this implementation have bottleneck -> IntermediateThread
        /// </summary>
        /// <param name="threadCtx"></param>
        static void ThreadEntry(object threadCtx)
        {
            //Random rnd = new Random();
            ThreadContext threadContext = threadCtx as ThreadContext;
            threadContext.ManagedID = threadContext.hThread.ManagedThreadId;
            while (!threadContext.StopThread)
            {
                if (threadContext.Job != null)
                {
                    PlanItem planned = threadContext.Job;

                    if (planned.JobEntry(threadContext, planned) == 1)
                    {
                        Thread.Sleep(maxSuspend);
                    }
                    else
                    {
                        Thread.Yield();
                        //Thread.Sleep(0000);
                    }
                }
                else
                {
                    Thread.Sleep(maxSuspend);
                }
                IntermediateThread(threadContext);
            }
            ExitThread(threadContext);
        }
        static void IsolatedThreadEntry(object o)
        {
            ThreadContext ti = o as ThreadContext;
            ti.ManagedID = ti.hThread.ManagedThreadId;
            PlanItem pi = ti.Job;
            pi.JobEntry(ti, pi);

            ExitThread(ti);
        }
    }
}
