using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskBroker;
using TaskQueue.Providers;

namespace QueueService
{
    public class ModProducer : IModIsolatedProducer
    {
        public const string ListeningOn = "http://localhost:82/";
        baseService appHost;
        public static Broker broker;

        public void Initialise(Broker context, TaskBroker.ModMod thisModule)
        {
            if (broker != null)
            {// except 
            }
            QueueService.ModProducer.broker = context;

            appHost = new baseService();
            appHost.Init();
        }
        public void IsolatedProducer(TItemModel parameters)
        {
            appHost.Start(ListeningOn);

            System.Console.WriteLine("AppHost queue services Created at {0}, listening on {1}",
                DateTime.Now, ListeningOn);
        }
        public void IsolatedProducerStop()
        {
            if (appHost != null)
            {
                appHost.Stop();
            }
        }

        public void Exit()
        {

        }

        public ModuleSelfTask[] RegisterTasks(ModMod thisModule)
        {
            ModuleSelfTask t = new ModuleSelfTask()
            {
                intervalType = TaskScheduler.IntervalType.isolatedThread,
                ModuleName = thisModule.UniqueName,
                NameAndDescription = "Host for web service[REST main service]"

            };
            return new ModuleSelfTask[] { t };
        }


        public string Name
        {
            get { return "REST-service"; }
        }

        public string Description
        {
            get { return ""; }
        }

    }
}
