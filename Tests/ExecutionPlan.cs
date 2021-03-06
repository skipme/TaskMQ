﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    using NUnit.Framework;
    using System.IO;
    using TaskScheduler;
    using TaskUniversum.Task;


    [TestFixture]
    public class ExecutionPlan
    {
        [Test]
        public void ExecutionPlan_CheckForExecution()
        {
            TaskScheduler.ThreadPool Scheduler = new TaskScheduler.ThreadPool();
            // check for job executed in same time
            uint localVar = 0;
            uint deferred_LocalVar = 0;
            uint short_term_var = 0;
            uint long_term_var = 0;
            bool notSucceeded = false;
            PlanItemEntryPoint job = (ThreadContext ti, PlanItem pi) =>
                     {
                         //Console.WriteLine("ExecutionPlan_CheckForExecution er {0}", System.Threading.Thread.CurrentThread.Name);

                         for (int i = 0; i < 1000; i++)
                         {
                             localVar++;
                         }
                         System.Threading.Thread.Sleep(400);
                         localVar = 0;
                         System.Threading.Thread.Sleep(100);
                         if (localVar != 0)
                             notSucceeded = true;

                         for (int i = 0; i < 1000; i++)
                         {
                             localVar++;
                         }
                         //Console.WriteLine("ExecutionPlan_CheckForExecution ex {0}", System.Threading.Thread.CurrentThread.Name);
                         return 1;
                     };
            //TaskScheduler.ExecutionPlan.LongTermBarrierSec = 3;
            List<PlanItem> TaskList = new List<PlanItem>()
            {
                new PlanItem(){
                     intervalType = IntervalType.withoutInterval,
                     NameAndDescription = "",
                     JobEntry = job
                },
                 new PlanItem(){
                     intervalType = IntervalType.intervalSeconds,
                     intervalValue = 1,// 2sec
                     NameAndDescription = "",
                     JobEntry = (ThreadContext ti, PlanItem pi) =>{short_term_var++; return 1;}
                },
                new PlanItem(){
                     intervalType = IntervalType.intervalSeconds,
                     intervalValue = 4,// more than long term barrier value
                     NameAndDescription = "",
                     JobEntry = (ThreadContext ti, PlanItem pi) =>{long_term_var++; return 1;}
                },
            };

            Scheduler.SetPlan(TaskList);
            Scheduler.ReWake();

            Scheduler.DeferJob((thread_, job_) => { deferred_LocalVar = 555; return 1; });

            System.Threading.Thread.Sleep(1500);
            
            Scheduler.SuspendAll();
            while (Scheduler.Activity)
            {
                System.Threading.Thread.Sleep(0);
            }
            //
            Assert.AreEqual(notSucceeded, false);

            Assert.AreNotEqual(localVar, 0);
            Assert.AreEqual(localVar, 1000);

            Assert.AreNotEqual(short_term_var, 0);

            Assert.AreEqual(deferred_LocalVar, 555);
        }

    }
}
