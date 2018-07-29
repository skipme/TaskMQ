using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using TaskScheduler;

namespace Tests
{
    public class Program
    {

        static int Main(string[] args)
        {
            new InMemoryQueue().MemQueue_Duplications();
            return 1;
            //double x = -128;
            //PerfCheck();
            //PerfCompare();
            //			Console.WriteLine((x));
            //			Console.WriteLine((byte)(x));
            //			Console.WriteLine(((long)x));
            //			Console.WriteLine((sbyte)((long)x));
            //			Console.WriteLine((sbyte)(x));

            //Scheduler_PayloadToOverallTime();

            //Scheduler_PayloadToOverallTime_VScale(100000, 1000, 5, 1, 1);
            //Scheduler_PayloadToOverallTime_VScale(100000, 1000, 5, 4, 1);
            //Scheduler_PayloadToOverallTime_VScale(100000, 1000, 5, 1, 100);
            //Scheduler_PayloadToOverallTime_VScale(100000, 1000, 5, 2, 100);
            //Scheduler_PayloadToOverallTime_VScale(100000, 1000, 5, 3, 100);
            //Scheduler_PayloadToOverallTime_VScale(100000, 1000, 5, 4, 100);
            //Scheduler_PayloadToOverallTime_VScale(100000, 1000, 5, 8, 100);
            //return 1;

            //for (uint xstep = 1; xstep < 5; xstep++)
            //{
            //    for (uint ysteps = 1; ysteps < 5; ysteps++)
            //    {
            //        for (uint zjobs = 1; zjobs < 10; zjobs++)
            //        {
            //            for (uint athreads = 1; athreads < 8; athreads++)
            //            {
            //                Scheduler_PayloadToOverallTime_VScale(
            //                    xstep * 100,
            //                    //ysteps*100,
            //                    (uint)Math.Pow(3, ysteps),
            //                    (uint)Math.Pow(2, zjobs),
            //                    athreads);
            //                    //(uint)Math.Pow(2, athreads));
            //                // zjobs * zjobs,
            //                // athreads * athreads);
            //            }
            //        }
            //    }
            //}
            for (uint zjobs = 1; zjobs < 10; zjobs++)
            {
                for (uint athreads = 1; athreads < 8; athreads++)
                {
                    Scheduler_PayloadToOverallTime_VScale(
                        100000,
                        1000,
                        (uint)Math.Pow(2, zjobs),
                        athreads,
                        100000);
                    //(uint)Math.Pow(2, athreads));
                    // zjobs * zjobs,
                    // athreads * athreads);
                }
            }
            //PerfQueue();

            //PerfDic();

            return 1;
        }
        static void Scheduler_PayloadToOverallTime_VScale(
            uint step
            , uint steps
            , uint jobsCount
            , uint ThreadsCount
            , uint stepSyncFactor)// interlock granulation actually
        {
            TaskScheduler.ThreadPool Scheduler = new TaskScheduler.ThreadPool(ThreadsCount);
            

            TimeSpan cspan = new TimeSpan();


            //uint localVar = 0, incVal = 10, maxVal = 10000000;
            int localVar = 0, incVal = (int)step, maxVal = (int)(step * steps), syncStep = (int)(step/stepSyncFactor);

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < maxVal; i++)
            {
                localVar++;
                //System.Threading.Interlocked.Increment(ref localVar);
                //if (localVar % incVal == 0)
                //    System.Threading.Thread.Sleep(0);
            }
            cspan = sw.Elapsed;
            //Console.WriteLine(cspan.TotalMilliseconds);

            //			sw = Stopwatch.StartNew();
            //			for (int i = 0; i < maxVal; i++) {
            //				localVar++;
            //			}
            //			cspan = sw.Elapsed;
            //			Console.WriteLine(cspan.TotalMilliseconds);

            localVar = 0;
            bool notSucceeded = true;
            Stopwatch swx_parallel = null;
            {
                Thread[] ths = new Thread[steps];
                swx_parallel = Stopwatch.StartNew();
                for (int i = 0; i < steps; i++)
                {
                    ths[i] = new Thread(() =>
                    {
                        //for (int x = 0; x < incVal; x++)
                        //{
                        //    System.Threading.Interlocked.Increment(ref localVar);
                        //}
                        int tempInc = 0;
                        for (int x = 0; x < syncStep; x++)
                        {
                            for (int y = 0; y < stepSyncFactor; y++)
                            {
                                tempInc++;
                            }
                            Interlocked.Add(ref localVar, tempInc);
                            tempInc = 0;
                        }
                        if (localVar >= maxVal)
                            notSucceeded = false;
                    });
                    ths[i].Start();
                }
                while (notSucceeded)
                {
                    System.Threading.Thread.Sleep(0);
                }
                swx_parallel.Stop(); 
            }
            TimeSpan payloadspan_parallel = swx_parallel.Elapsed;

            localVar = 0;
            notSucceeded = true;
            
            PlanItemEntryPoint job = (ThreadContext ti, PlanItem pi) =>
            {
                //Console.WriteLine("ex started {0} {1}", ti.hThread.Name, localVar);
                //for (int i = 0; i < incVal; i++)
                //{
                //    //localVar++;
                //    System.Threading.Interlocked.Increment(ref localVar);
                //}
                int tempInc = 0;
                for (int x = 0; x < syncStep; x++)
                {
                    for (int y = 0; y < stepSyncFactor; y++)
                    {
                        tempInc++;
                    }
                    Interlocked.Add(ref localVar, tempInc);
                    tempInc = 0;
                }
                //System.Threading.Thread.Sleep(0);
                if (localVar >= maxVal)
                    notSucceeded = false;
                //Console.WriteLine("succ executed");
                return -1;
            };

            List<PlanItem> TaskList = new List<PlanItem>();

            for (int i = 0; i < jobsCount; i++)
            {
                TaskList.Add(new PlanItem()
                {
                    intervalType = TaskUniversum.Task.IntervalType.withoutInterval,
                    NameAndDescription = "",
                    JobEntry = job
                });
            }

            //Console.WriteLine("jobs: {0}", TaskList.Count);

            Stopwatch swx = Stopwatch.StartNew();
            Scheduler.SetPlan(TaskList);
            Scheduler.ReWake();

            while (notSucceeded)
            {
                System.Threading.Thread.Sleep(0);
            }
            swx.Stop();
            TimeSpan payloadspan = swx.Elapsed;
            Scheduler.SuspendAll();
            //Console.WriteLine("e act");
            while (Scheduler.Activity)
            {
                System.Threading.Thread.Sleep(0);
            }

            //

            //Console.WriteLine("{0} by {2} --- {3:.00} :: {4} {5} {6} {7}",
            //Console.WriteLine("{0} {2} {9:.00} {3:.00} {8:.00} {4} {5} {6} {7}",
            //    cspan.TotalMilliseconds, cspan.TotalMilliseconds,// * (maxVal / incVal),
            //    payloadspan.TotalMilliseconds,
            //    //(payloadspan.TotalMilliseconds * (maxVal / incVal)) / cspan.TotalMilliseconds);
            //    payloadspan.TotalMilliseconds / cspan.TotalMilliseconds,
            //     step, steps, jobsCount, ThreadsCount,
            //     payloadspan.TotalMilliseconds / payloadspan_parallel.TotalMilliseconds,
            //     payloadspan_parallel.TotalMilliseconds);

            Console.WriteLine("{0:0.00} {1:0.00} {2:0.00} {3:0.00} {4:0.00} {5} {6} {7}"
                , cspan.TotalMilliseconds
                , payloadspan.TotalMilliseconds
                , payloadspan_parallel.TotalMilliseconds
                , payloadspan.TotalMilliseconds / cspan.TotalMilliseconds
                , payloadspan.TotalMilliseconds / payloadspan_parallel.TotalMilliseconds
                , jobsCount
                , ThreadsCount
                , stepSyncFactor);


        }
        static void Scheduler_PayloadToOverallTime()
        {
            TaskScheduler.ThreadPool Scheduler = new TaskScheduler.ThreadPool();

            TimeSpan cspan = new TimeSpan();


            uint localVar = 0, incVal = 10, maxVal = 10000;

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < maxVal; i++)
            {
                localVar++;
                if (localVar % incVal == 0)
                    System.Threading.Thread.Sleep(1);
            }
            cspan = sw.Elapsed;
            Console.WriteLine(cspan.TotalMilliseconds);

            //			sw = Stopwatch.StartNew();
            //			for (int i = 0; i < maxVal; i++) {
            //				localVar++;
            //			}
            //			cspan = sw.Elapsed;
            //			Console.WriteLine(cspan.TotalMilliseconds);

            localVar = 0;

            bool notSucceeded = true;
            PlanItemEntryPoint job = (ThreadContext ti, PlanItem pi) =>
            {
                //Console.WriteLine("ex started {0} {1}", ti.hThread.Name, localVar);
                for (int i = 0; i < incVal; i++)
                {
                    localVar++;

                }
                System.Threading.Thread.Sleep(1);
                if (localVar >= maxVal)
                    notSucceeded = false;
                //Console.WriteLine("succ executed");
                return -1;
            };
            List<PlanItem> TaskList = new List<PlanItem>() {
				new PlanItem() {
					intervalType = TaskUniversum.Task.IntervalType.withoutInterval,
					NameAndDescription = "",
					JobEntry = job
				},
			};


            Scheduler.ReWake();
            Stopwatch swx = Stopwatch.StartNew();
            Scheduler.SetPlan(TaskList);

            while (notSucceeded)
            {
                System.Threading.Thread.Sleep(0);
            }
            TimeSpan payloadspan = swx.Elapsed;
            Scheduler.SuspendAll();
            Console.WriteLine("e act");
            //while (Scheduler.Activity)
            //{
            //}

            //

            Console.WriteLine("{0}({1}) by {2} --- {3:.00}",
                cspan.TotalMilliseconds, cspan.TotalMilliseconds,// * (maxVal / incVal),
                payloadspan.TotalMilliseconds,
                //(payloadspan.TotalMilliseconds * (maxVal / incVal)) / cspan.TotalMilliseconds);
                payloadspan.TotalMilliseconds / cspan.TotalMilliseconds);

        }
        static void PerfDic()
        {
            int maxcnt = 100;
            Dictionary<string, int> ex = new Dictionary<string, int>();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < maxcnt; i++)
            {
                ex.Add("left" + i.ToString(), 1);
            }
            watch.Stop();
            Console.WriteLine("dic add {0:.000}ms", watch.Elapsed.TotalMilliseconds);
            watch.Restart();
            for (int i = 0; i < maxcnt; i++)
            {
                int x = ex["left" + i.ToString()];
            }
            watch.Stop();
            Console.WriteLine("dic lookup {0:.000}ms", watch.Elapsed.TotalMilliseconds);

            System.Collections.Hashtable sht = new System.Collections.Hashtable();

            watch.Restart();
            for (int i = 0; i < maxcnt; i++)
            {
                sht.Add("left" + i.ToString(), 1);
            }
            watch.Stop();
            Console.WriteLine("\nhsht add {0:.000}ms", watch.Elapsed.TotalMilliseconds);

            watch.Restart();
            for (int i = 0; i < maxcnt; i++)
            {
                object x = sht["left" + i.ToString()];
            }
            watch.Stop();
            Console.WriteLine("hsht lookup {0:.000}ms", watch.Elapsed.TotalMilliseconds);

            int maxinstances = 100000;
            watch.Restart();
            for (int i = 0; i < maxinstances; i++)
            {
                ex = new Dictionary<string, int>();
            }
            watch.Stop();
            Console.WriteLine("\ndic instances {0:.000}ms", watch.Elapsed.TotalMilliseconds);
            watch.Restart();
            for (int i = 0; i < maxinstances; i++)
            {
                sht = new System.Collections.Hashtable();
            }
            watch.Stop();
            Console.WriteLine("hsht instances {0:.000}ms", watch.Elapsed.TotalMilliseconds);


        }
        //static void PerfCheck()
        //{
        //    CustomMessagesCompare.SomeExtMessage msg = new CustomMessagesCompare.SomeExtMessage()
        //    {
        //        field1 = 111,
        //        field2 = 15
        //    };

        //    Dictionary<string, object> a = msg.GetHolder();

        //    TaskQueue.TQItemSelector sel = new TaskQueue.TQItemSelector
        //        ("field1", TaskQueue.TQItemSelectorSet.Ascending)
        //        .Rule("field2", 15L, true);

        //    Stopwatch watch = new Stopwatch();
        //    TaskQueue.Providers.TaskMessage.InternalCheckDictionary chk =
        //        (TaskQueue.Providers.TaskMessage.InternalCheckDictionary)
        //        TaskQueue.Providers.TaskMessage.MakeCheckerDictionary(sel);
        //    watch.Start();
        //    for (int i = 0; i < 1000000; i++)
        //    {
        //        chk(a);
        //    }
        //    watch.Stop();
        //    Console.WriteLine("compiled {0:.00}ms", watch.Elapsed.TotalMilliseconds);
        //    watch.Restart();
        //    for (int i = 0; i < 1000000; i++)
        //    {
        //        TaskQueue.Providers.TaskMessage.CheckWithSelector(a, sel);
        //    }
        //    watch.Stop();
        //    Console.WriteLine("native {0:.00}ms", watch.Elapsed.TotalMilliseconds);
        //}
        //static void PerfCompare()
        //{
        //    CustomMessagesCompare.SomeExtMessage msg = new CustomMessagesCompare.SomeExtMessage()
        //    {
        //        field1 = 111,
        //        field2 = 15
        //    };
        //    CustomMessagesCompare.SomeExtMessage msg2 = new CustomMessagesCompare.SomeExtMessage()
        //    {
        //        field1 = 111,
        //        field2 = 77
        //    };
        //    Dictionary<string, object> a = msg.GetHolder();
        //    Dictionary<string, object> b = msg2.GetHolder();
        //    TaskQueue.TQItemSelector sel = new TaskQueue.TQItemSelector
        //        ("field1", TaskQueue.TQItemSelectorSet.Ascending)
        //        .Rule("field2", TaskQueue.TQItemSelectorSet.Descending);

        //    Stopwatch watch = new Stopwatch();
        //    TaskQueue.Providers.TaskMessage.InternalComparableDictionary cmp = (TaskQueue.Providers.TaskMessage.InternalComparableDictionary)
        //         TaskQueue.Providers.TaskMessage.MakeComparatorDictionary(sel);
        //    watch.Start();
        //    for (int i = 0; i < 1000000; i++)
        //    {
        //        cmp(a, b);
        //    }
        //    watch.Stop();
        //    Console.WriteLine("compiled {0:.00}ms", watch.Elapsed.TotalMilliseconds);
        //    watch.Restart();
        //    for (int i = 0; i < 1000000; i++)
        //    {
        //        TaskQueue.Providers.TaskMessage.CompareWithSelector(a, b, sel);
        //    }
        //    watch.Stop();
        //    Console.WriteLine("native {0:.00}ms", watch.Elapsed.TotalMilliseconds);

        //    Tests.CustomMessagesCompare.InternalComparable cmp2 = (Tests.CustomMessagesCompare.InternalComparable)
        //        msg.MakeComparator(sel, typeof(Tests.CustomMessagesCompare.InternalComparable));


        //    watch.Restart();
        //    for (int i = 0; i < 1000000; i++)
        //    {
        //        cmp2(msg, msg2);
        //    }
        //    watch.Stop();
        //    Console.WriteLine("compiled native {0:.00}ms", watch.Elapsed.TotalMilliseconds);
        //}
        static void PerfQueue()
        {
            CustomMessagesCompare.SomeExtMessage msg = new CustomMessagesCompare.SomeExtMessage()
            {
                field1 = 111,
                field2 = 15
            };
            CustomMessagesCompare.SomeExtMessage msg2 = new CustomMessagesCompare.SomeExtMessage()
            {
                field1 = 111,
                field2 = 77
            };
            Dictionary<string, object> a = msg.GetHolder();
            Dictionary<string, object> b = msg2.GetHolder();
            TaskQueue.TQItemSelector sel = new TaskQueue.TQItemSelector("field1", TaskQueue.TQItemSelectorSet.Ascending)
                .Rule("field2", TaskQueue.TQItemSelectorSet.Descending)
                .Rule("Processed", false, true);

            TaskQueue.Providers.MemQueue mq = new TaskQueue.Providers.MemQueue();
            mq.SetSelector(sel);

            SecQueue.SecQueue mq2 = new SecQueue.SecQueue();
            mq2.SetSelector(sel);
            const int counting = 50000;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Dictionary<string, object> baseData = msg.GetHolder();
            Random rnd = new Random();
            List<TaskQueue.Providers.TaskMessage> data = new List<TaskQueue.Providers.TaskMessage>();
            for (int i = 0; i < counting; i++)
            {
                baseData["field1"] = rnd.Next();
                baseData["field2"] = rnd.Next();
                data.Add(new TaskQueue.Providers.TaskMessage(baseData));
            }
            watch.Stop();
            Console.WriteLine("populate {0:.00}ms {1} avg [{2:.0}] msg per second", watch.Elapsed.TotalMilliseconds, counting, (1000.0 / watch.Elapsed.TotalMilliseconds) * counting);

            watch.Restart();
            for (int i = 0; i < counting; i++)
            {
                mq.Push(data[i]);
                if (i % 2 == 0)
                {
                    var itm = mq.GetItem();
                    itm.Processed = true;
                    mq.UpdateItem(itm);
                }
            }

            watch.Stop();
            Console.WriteLine("sortedset {0:.00}ms {1} avg [{2:.0}] msg per second", watch.Elapsed.TotalMilliseconds, mq.GetQueueLength(), (1000.0 / watch.Elapsed.TotalMilliseconds) * counting);
            watch.Restart();

            for (int i = 0; i < counting; i++)
            {
                mq2.Push(data[i]);
                if (i % 2 == 0)
                {
                    var itm = mq2.GetItem();
                    itm.Processed = true;
                    mq2.UpdateItem(itm);
                }
            }
            watch.Stop();
            Console.WriteLine("secset {0:.00}ms {1} avg [{2:.0}] msg per second", watch.Elapsed.TotalMilliseconds, mq2.GetQueueLength(), (1000.0 / watch.Elapsed.TotalMilliseconds) * counting);

            return;
            MongoQueue.MongoDbQueue mq3 = new MongoQueue.MongoDbQueue();
            mq3.InitialiseFromModel(null, new TaskQueue.Providers.QueueConnectionParameters("")
            {
                specParams = (new MongoQueue.MongoQueueParams()
                {
                    Collection = "test",
                    Database = "test",
                    ConnectionString = "mongodb://localhost:27017/Messages?safe=true"
                })
            });
            mq3.SetSelector(sel);

            watch.Restart();
            for (int i = 0; i < counting; i++)
            {
                mq3.Push(data[i]);
                if (i % 2 == 0)
                {
                    var itm = mq3.GetItem();
                    itm.Processed = true;
                    mq3.UpdateItem(itm);
                }
            }
            watch.Stop();
            Console.WriteLine("mongo {0:.00}ms {1} avg [{2:.0}] msg per second", watch.Elapsed.TotalMilliseconds, mq3.GetQueueLength(), (1000.0 / watch.Elapsed.TotalMilliseconds) * counting);
        }
    }
}
