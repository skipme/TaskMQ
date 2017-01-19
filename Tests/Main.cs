using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tests
{
    public class Program
    {

        static int Main(string[] args)
        {
            //PerfCheck();
            //PerfCompare();
            //PerfQueue();

            PerfDic();

            return 1;
        }
        static void PerfDic()
        {
            int maxcnt = 15;
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
            Console.WriteLine("hsht add {0:.000}ms", watch.Elapsed.TotalMilliseconds);

            watch.Restart();
            for (int i = 0; i < maxcnt; i++)
            {
                object x = sht["left" + i.ToString()];
            }
            watch.Stop();
            Console.WriteLine("hsht lookup {0:.000}ms", watch.Elapsed.TotalMilliseconds);

        }
        static void PerfCheck()
        {
            CustomMessagesCompare.SomeExtMessage msg = new CustomMessagesCompare.SomeExtMessage()
            {
                field1 = 111,
                field2 = 15
            };

            Dictionary<string, object> a = msg.GetHolder();

            TaskQueue.TQItemSelector sel = new TaskQueue.TQItemSelector
                ("field1", TaskQueue.TQItemSelectorSet.Ascending)
                .Rule("field2", 15L, true);

            Stopwatch watch = new Stopwatch();
            TaskQueue.Providers.TaskMessage.InternalCheckDictionary chk =
                (TaskQueue.Providers.TaskMessage.InternalCheckDictionary)
                TaskQueue.Providers.TaskMessage.MakeCheckerDictionary(sel);
            watch.Start();
            for (int i = 0; i < 1000000; i++)
            {
                chk(a);
            }
            watch.Stop();
            Console.WriteLine("compiled {0:.00}ms", watch.Elapsed.TotalMilliseconds);
            watch.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                TaskQueue.Providers.TaskMessage.CheckWithSelector(a, sel);
            }
            watch.Stop();
            Console.WriteLine("native {0:.00}ms", watch.Elapsed.TotalMilliseconds);
        }
        static void PerfCompare()
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
            TaskQueue.TQItemSelector sel = new TaskQueue.TQItemSelector
                ("field1", TaskQueue.TQItemSelectorSet.Ascending)
                .Rule("field2", TaskQueue.TQItemSelectorSet.Descending);

            Stopwatch watch = new Stopwatch();
            TaskQueue.Providers.TaskMessage.InternalComparableDictionary cmp = (TaskQueue.Providers.TaskMessage.InternalComparableDictionary)
                 TaskQueue.Providers.TaskMessage.MakeComparatorDictionary(sel);
            watch.Start();
            for (int i = 0; i < 1000000; i++)
            {
                cmp(a, b);
            }
            watch.Stop();
            Console.WriteLine("compiled {0:.00}ms", watch.Elapsed.TotalMilliseconds);
            watch.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                TaskQueue.Providers.TaskMessage.CompareWithSelector(a, b, sel);
            }
            watch.Stop();
            Console.WriteLine("native {0:.00}ms", watch.Elapsed.TotalMilliseconds);

            Tests.CustomMessagesCompare.InternalComparable cmp2 = (Tests.CustomMessagesCompare.InternalComparable)
                msg.MakeComparator(sel, typeof(Tests.CustomMessagesCompare.InternalComparable));


            watch.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                cmp2(msg, msg2);
            }
            watch.Stop();
            Console.WriteLine("compiled native {0:.00}ms", watch.Elapsed.TotalMilliseconds);
        }
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
            TaskQueue.TQItemSelector sel = new TaskQueue.TQItemSelector
                ("field1", TaskQueue.TQItemSelectorSet.Ascending)
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
            Console.WriteLine("populate {0:.00}ms {1}", watch.Elapsed.TotalMilliseconds, counting);

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
            Console.WriteLine("sortedset {0:.00}ms {1}", watch.Elapsed.TotalMilliseconds, mq.GetQueueLength());
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
            Console.WriteLine("secset {0:.00}ms {1}", watch.Elapsed.TotalMilliseconds, mq2.GetQueueLength());
            //List<TaskQueue.Providers.TaskMessage> l1 = new List<TaskQueue.Providers.TaskMessage>();
            //List<TaskQueue.Providers.TaskMessage> l2 = new List<TaskQueue.Providers.TaskMessage>();
            //while (true)
            //{
            //    TaskQueue.Providers.TaskMessage lo = mq.GetItem();
            //    if (lo == null) break;
            //    lo.Processed = true;
            //    mq.UpdateItem(lo);
            //    l1.Add(lo);
            //}
            //while (true)
            //{
            //    TaskQueue.Providers.TaskMessage lo = mq2.GetItem();
            //    if (lo == null) break;
            //    lo.Processed = true;
            //    mq2.UpdateItem(lo);
            //    l2.Add(lo);
            //}
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
            Console.WriteLine("mongo {0:.00}ms {1}", watch.Elapsed.TotalMilliseconds, mq3.GetQueueLength());
        }
    }
}
