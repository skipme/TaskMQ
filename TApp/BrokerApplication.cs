using MongoQueue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using TaskBroker.Configuration;
using TaskScheduler;
using TaskUniversum;

namespace TaskBroker
{
    [SecurityPermission(SecurityAction.Demand, Infrastructure = true)]
    public class BrokerApplication : MarshalByRefObject, IDisposable
    {
        static ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();
        ManualResetEvent Signal;
        TaskBroker.Broker broker;

        volatile bool Signalling = false;

        public void Dispose()
        {
        }
        void Restart()
        {
            if (Signalling)
            {
                logger.Warning("Signal invocation already initiated");
                return;
            }
            Signalling = true;
            Thread th = new Thread((object o) =>
            {
                broker.StopBroker(); // ! stop scheduler and other isolated threads
                logger.Debug("Signal to appdomain set by restart...");
                Signal.Set();
            });
            th.Start();
        }
        void Reset()
        {
            Thread th = new Thread((object o) =>
            {
                broker.StopBroker(); // ! stop scheduler and other isolated threads
                broker.RevokeBroker(false, true, true);
            });
            th.Start();
        }
        public void Run(ManualResetEvent signal, bool benchConf, bool customDomain)
        {
            if (customDomain)
            {
                // reinitiate tape within domain
                TaskBroker.Logger.CommonTape tape = new TaskBroker.Logger.CommonTape(
                    new TaskBroker.Logger.LoggerEndpoint[]{
                    new TaskBroker.Logger.ConsoleEndpoint(),
                    new TaskBroker.Logger.FileEndpoint("log.txt", true)
                });

                TaskUniversum.ModApi.ScopeLogger.RegisterCommonTape(tape);
            }
            //if (!AttachConsole(-1))
            ////if (FreeConsole())
            //{ 
            //    AllocConsole();
            //    logger.Warning("consoleattached");
            //}
            broker = new Broker(Restart, Reset);

            BsonBenchService.ModProducer amqm = new BsonBenchService.ModProducer();
            QueueService.ModProducer m = new QueueService.ModProducer();// todo: force loading local dep's
            BenchModules.ModConsumer cons = new BenchModules.ModConsumer();
            //EmailConsumer.ModConsumer mc = new EmailConsumer.ModConsumer();

            //m.Description = "";
            //cons.Description = "";

            broker.RevokeBroker(benchConf, true, false);
            this.Signal = signal;
            exportConfiguration();
        }
#if !MONO
        //[System.Runtime.InteropServices.DllImport("kernel32.dll")]
        //private static extern bool AllocConsole();

        //[System.Runtime.InteropServices.DllImport("kernel32.dll")]
        //private static extern bool AttachConsole(int pid);

        //[System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        //private static extern bool FreeConsole();
#endif
        /// <summary>
        /// Many runtime configuration declared, dump object model of it if required
        /// </summary>
        public void exportConfiguration()
        {
            File.WriteAllBytes("main.json", BrokerConfiguration.ExtractFromBroker(broker).SerialiseJson());
            File.WriteAllBytes("modules.json", BrokerConfiguration.ExtractModulesFromBroker(broker).SerialiseJson());
            File.WriteAllBytes("assemblies.json", BrokerConfiguration.ExtractAssemblysFromBroker(broker).SerialiseJson());
        }
    }
}
