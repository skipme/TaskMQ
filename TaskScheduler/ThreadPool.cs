using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TaskUniversum.Task;

namespace TaskScheduler
{
    public class ThreadItem
    {
        public bool Isolated { get; set; }
        public Thread hThread { get; set; }
        public bool HasJob { get; set; }
        public bool JobComplete { get; set; }

        public bool StopThread { get; set; }
        public bool StoppedThread { get; set; }

        public int ManagedID { get; set; }

        public PlanItem ExecutionContext { get; set; }
        public ExecutionPlan rootPlan { get; set; }
    }

    public class ThreadPool
    {
        const int maxThreads = 8;
        List<ThreadItem> threads = new List<ThreadItem>();
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
                foreach (ThreadItem t in threads)
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
                ThreadItem ti = new ThreadItem()
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
            foreach (ThreadItem t in threads)
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
                thread.Name = "iso " + pi.NameAndDescription;
                ThreadItem ti = new ThreadItem()
                {
                    hThread = thread,
                    rootPlan = plan,
                    ExecutionContext = pi,
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

        static void IntermediateThread(ThreadItem ti)
        {
            lock (ti.rootPlan)
            {
                if (ti.HasJob && ti.JobComplete)
                {
                    ti.ExecutionContext.ExucutingNow = false;
                }
                PlanItem pi = ti.rootPlan.Next();
                ti.ExecutionContext = pi;
                if (ti.HasJob = pi != null)
                {
                    ti.ExecutionContext.ExucutingNow = true;
                }
                ti.JobComplete = false;
            }
            //Console.WriteLine("Intermediate {0} {1}", ti.ManagedID, ti.hThread.IsThreadPoolThread);
        }
        static void ExitThread(ThreadItem ti)
        {
            ti.StoppedThread = true;
            //Console.WriteLine("Exit" + ti.ManagedID);
        }
        static void ThreadEntry(object o)
        {
            ThreadItem ti = o as ThreadItem;
            ti.ManagedID = ti.hThread.ManagedThreadId;
            while (!ti.StopThread)
            {
                if (ti.HasJob)
                {
                    PlanItem pi = ti.ExecutionContext;

                    pi.planEntry(ti, pi);

                    pi.LastExecutionTime = DateTime.Now;
                    ti.JobComplete = true;
                    Thread.Sleep(0000);
                }
                else
                {
                    Thread.Sleep(10);
                }
                IntermediateThread(ti);
            }
            ExitThread(ti);
        }
        static void IsolatedThreadEntry(object o)
        {
            ThreadItem ti = o as ThreadItem;
            ti.ManagedID = ti.hThread.ManagedThreadId;
            PlanItem pi = ti.ExecutionContext;
            pi.planEntry(ti, pi);

            ExitThread(ti);
        }
    }
}
