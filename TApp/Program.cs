using MongoQueue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TaskBroker;
using TaskBroker.Configuration;
using TaskScheduler;


namespace TApp
{
    class Program
    {
        static ManualResetEvent sync;
        //netsh http add urlacl url=http://+:82/ user=User
        static void RestartAsAdmin()
        {
            var startInfo = new ProcessStartInfo("TApp.exe") { Verb = "runas" };
            Process.Start(startInfo);
            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
			SourceControl.Build.AssemblyBuilder ab = new SourceControl.Build.AssemblyBuilder (
				"/home/cerriun/TaskMQ-master/QueueService/QueueService.csproj");
			ab.BuildProject ();
			return;
            TaskBroker.Logger.CommonTape tape = new TaskBroker.Logger.CommonTape(new TaskBroker.Logger.LoggerEndpoint[]{
                new TaskBroker.Logger.ConsoleEndpoint()
            });
            TaskUniversum.ModApi.ScopeLogger.RegisterCommonTape(tape);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                BrokerApplication ba = new BrokerApplication();
                ba.Run(mre);
                mre.WaitOne();
            }
            else
            {
                ApplicationKeeper.AppdomainLoop();
            }
            Console.WriteLine("Application Main entry exit.");
            Console.ReadLine();
            return;
        }
    }
}
