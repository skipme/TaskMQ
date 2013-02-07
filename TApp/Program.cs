using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TApp
{
    class Program
    {
        [TaskQueue.AQueueModel()]
        public class Tclass
        {
            public string DoYouSeeMe { get; set; }
            public string SDoYouSeeMe { get; set; }
        }

        public class Mclass : TaskQueue.Providers.TaskMessage
        {
            public Mclass() : base("drug") { }
            public string DoYouSeeMe { get; set; }
            public string SDoYouSeeMe { get; set; }
        }

        static void Main(string[] args)
        {
            //TaskQueue.QueueModel qm = new TaskQueue.QueueModel(typeof(Tclass));
            //Mclass ms = new Mclass();
            //ms.DoYouSeeMe = "habba";

            //Tclass tv = new Tclass();
            //qm.MapPropsToModel<Tclass>(ms, tv);

            //System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();

            //Mclass c = new Mclass()
            //{
            //    DoYouSeeMe = "hackMe",
            //    SDoYouSeeMe = "ok"
            //};
            //w.Start();
            ////c.EnumerateKeys();
            //c.GetValues().ToArray();
            //w.Stop();

            //Console.WriteLine("{0}", w.ElapsedMilliseconds);

            //w.Start();
            ////c.EnumerateKeys();
            //c.GetValues().ToArray();
            //w.Stop();
            //Console.WriteLine("{0}", w.ElapsedMilliseconds);

            //w.Start();
            ////c.EnumerateKeys();
            //c.GetValues().ToArray();
            //w.Stop();
            //Console.WriteLine("{0}", w.ElapsedMilliseconds);

            //w.Start();
            ////c.EnumerateKeys();
            //c.GetValues().ToArray();
            //w.Stop();
            //Console.WriteLine("{0}", w.ElapsedMilliseconds);

            //w.Start();
            ////c.EnumerateKeys();
            //c.GetValues().ToArray();
            //w.Stop();
            //Console.WriteLine("{0}", w.ElapsedMilliseconds);




            //TaskScheduler.ThreadPool.Allocate();
            //Console.ReadLine();
            //TaskScheduler.ThreadPool.Allocate();
            //Console.ReadLine();
            //TaskScheduler.ThreadPool.Allocate();
            //Console.ReadLine();
            //TaskScheduler.ThreadPool.Allocate();
            //Console.ReadLine();
            //TaskScheduler.ThreadPool.Allocate();
            //TaskScheduler.ThreadPool.SuspendAll();

            //TaskScheduler.PlanItem plan1 = new TaskScheduler.PlanItem()
            //{
            //    intervalType = TaskScheduler.IntervalType.withoutInterval,
            //    planEntry = (TaskScheduler.PlanItem pi) => {  }
            //};
            //TaskScheduler.PlanItem plan2 = new TaskScheduler.PlanItem()
            //{
            //    intervalType = TaskScheduler.IntervalType.everyCustomMilliseconds,
            //    intervalValue = 500,
            //    planEntry = (TaskScheduler.PlanItem pi) => { Console.Write("+"); }
            //};
            //TaskScheduler.ThreadPool tp = new TaskScheduler.ThreadPool();
            //tp.SetPlan(plan1, plan2);
            //tp.Allocate();

            TaskBroker.Broker b = new TaskBroker.Broker();
            QueueService.ModProducer.Initialise(b);

            Console.Read();
        }
    }
}
