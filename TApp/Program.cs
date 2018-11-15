using MongoQueue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using System.Linq;
using System.Text;
using System.Threading;
using TaskBroker;
using TaskBroker.Configuration;
using TaskScheduler;


namespace TApp
{
    class Program
    {
        const string logFileProg = "tapp-log.txt";
        static void Main(string[] args)
        {
            TaskBroker.Logger.CommonTape tape = new TaskBroker.Logger.CommonTape(new TaskBroker.Logger.LoggerEndpoint[]{
                    new TaskBroker.Logger.ConsoleEndpoint(),
                    new TaskBroker.Logger.FileEndpoint(logFileProg)
                });

            TaskUniversum.ModApi.ScopeLogger.RegisterCommonTape(tape);
            bool benchConfiguration = true;// Array.IndexOf(args, "benchTasks") >= 0;// for plan or other params depends on tasks calculation

            if (false)
            //if (Array.IndexOf(args, "inline") >= 0 || System.Diagnostics.Debugger.IsAttached)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                BrokerApplication ba = new BrokerApplication();
                ba.Run(mre, benchConfiguration, false);
                mre.WaitOne();
            }
            else
            {
                ApplicationKeeper.AppdomainLoop(benchConfiguration);
            }
            Console.WriteLine("! Application MAIN EXIT.");
            Console.ReadLine();
            return;
        }
        static ManualResetEvent sync;
        //netsh http add urlacl url=http://+:82/ user=User
        static void RestartAsAdmin()
        {
            var startInfo = new ProcessStartInfo("TApp.exe") { Verb = "runas" };
            Process.Start(startInfo);
            Environment.Exit(0);
        }

    }
}
