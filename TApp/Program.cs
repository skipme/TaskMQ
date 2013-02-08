using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace TApp
{
    class Program
    {
        //netsh http add urlacl url=http://+:82/ user=User
        static void RestartAsAdmin()
        {
            var startInfo = new ProcessStartInfo("TApp.exe") { Verb = "runas" };
            Process.Start(startInfo);
            Environment.Exit(0);
        }
        static void Main(string[] args)
        {
            var prefix = QueueService.ModProducer.ListeningOn;
            var username = Environment.GetEnvironmentVariable("USERNAME");
            var userdomain = Environment.GetEnvironmentVariable("USERDOMAIN");
            Console.WriteLine("  netsh http add urlacl url={0} user={1}\\{2} listen=yes",
                    prefix, userdomain, username);

            TaskBroker.Broker b = new TaskBroker.Broker();
            //QueueService.ModProducer.Initialise(b);
            TaskBroker.ModMod m = new TaskBroker.ModMod()
            {
                InitialiseEntry = QueueService.ModProducer.Initialise,
                ExitEntry = QueueService.ModProducer.Exit,
                ModAssembly = typeof(QueueService.ModProducer).Assembly,
            };
            b.RegistrateModule(m);

            Console.Read();
        }
    }
}
