using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace QueueService
{
    public static class ModProducer
    {
        public const string ListeningOn = "http://localhost:82/";
        public static TaskBroker.Broker broker;

        public static void Initialise(TaskBroker.Broker brokerInterface, TaskBroker.ModMod thisModule)
        {
            broker = brokerInterface;
            thisModule.Producer = IsolatedProducer;
            thisModule.AcceptedParameters = new TaskQueue.QueueItemModel(typeof(TaskQueue.Providers.TItemModel));
            broker.RegistrateTask("http main service", "null", thisModule.UniqueName, "Host for web service", TaskScheduler.IntervalType.isolatedThread, 0);
        }
        public static void IsolatedProducer(TItemModel parameters)
        {
            var appHost = new baseService();
            appHost.Init();
            appHost.Start(ListeningOn);

            System.Console.WriteLine("AppHost queue services Created at {0}, listening on {1}",
                DateTime.Now, ListeningOn);
        }
        public static void Exit()
        {
        }
    }
}
