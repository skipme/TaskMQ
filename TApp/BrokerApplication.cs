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
                broker.RevokeBroker(true, true);
            });
            th.Start();
        }
        public void Run(ManualResetEvent signal)
        {
            broker = new Broker(Restart, Reset);

            QueueService.ModProducer m;// todo: force loading local dep's
            BenchModules.ModConsumer cons;

            broker.RevokeBroker(true);
            this.Signal = signal;
            //exportConfiguration();
        }

        //public void exportConfiguration()
        //{
        //    File.WriteAllBytes("main.json", BrokerConfiguration.ExtractFromBroker(broker).SerialiseJson());
        //    File.WriteAllBytes("modules.json", BrokerConfiguration.ExtractModulesFromBroker(broker).SerialiseJson());
        //    File.WriteAllBytes("assemblys.json", BrokerConfiguration.ExtractAssemblysFromBroker(broker).SerialiseJson());
        //}
    }
}
