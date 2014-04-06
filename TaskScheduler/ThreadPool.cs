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
        public bool HasJob { get; set; }
        public bool JobComplete { get; set; }

        public bool StopThread { get; set; }
        public bool StoppedThread { get; set; }

        public int ManagedID { get; set; }

        public PlanItem Job { get; set; }
        public ExecutionPlan rootPlan { get; set; }
    }

    public class ThreadPool
    {
        const int maxThreads = 8;
        List<ThreadContext> threads = new List<ThreadContext>();
        ExecutionPlan plan = new ExecutionPlan();
        public ThreadPool() { Revoke(); }
        // Throughput tune
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
                thread.Name = "TaskScheduler Thread#" + i;
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
            lock (plan)
            {
                plan.PlanComponents = setPlanItems;
                plan.Create();
            }
        }
        public void SetPlan(params PlanItem[] args)
        {
            lock (plan)
            {
                plan.PlanComponents = args.ToList();
                plan.Create();
            }
        }
        public void SetPlan(IEnumerable<PlanItem> planItems)
        {
            lock (plan)
            {
                plan.PlanComponents = planItems.ToList();
                plan.Create();
            }
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
            lock (ti.rootPlan)
            {
                if (ti.HasJob && ti.JobComplete)
                {
                    ti.Job.ExucutingNow = false;
                }
                PlanItem pi = ti.rootPlan.Next();
                ti.Job = pi;
                if (ti.HasJob = pi != null)
                {
                    ti.Job.ExucutingNow = true;
                }
                ti.JobComplete = false;
            }
            //Console.WriteLine("Intermediate {0} {1}", ti.ManagedID, ti.hThread.IsThreadPoolThread);
        }
        static void ExitThread(ThreadContext ti)
        {
            ti.StoppedThread = true;
            //Console.WriteLine("Exit" + ti.ManagedID);
        }
        /// <summary>
        /// All threads do it ;
        /// this implementation have bottleneck -> IntermediateThread
        /// </summary>
        /// <param name="threadCtx"></param>
        static void ThreadEntry(object threadCtx)
        {
            ThreadContext threadContext = threadCtx as ThreadContext;
            threadContext.ManagedID = threadContext.hThread.ManagedThreadId;
            while (!threadContext.StopThread)
            {
                if (threadContext.HasJob)
                {
                    PlanItem planned = threadContext.Job;

                    planned.JobEntry(threadContext, planned);

                    planned.LastExecutionTime = DateTime.Now;
                    threadContext.JobComplete = true;
                    Thread.Sleep(0000);
                }
                //else
                //{
                    Thread.Sleep(10);
                //}
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
