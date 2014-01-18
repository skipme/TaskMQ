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
