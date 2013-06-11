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
        public TaskBroker.Broker broker;

        public void Initialise(TaskBroker.Broker brokerInterface, TaskBroker.ModMod thisModule)
        {
            broker = brokerInterface;
            //thisModule.Producer = IsolatedProducer;
            thisModule.Parameters = new TaskQueue.QueueItemModel(typeof(TaskQueue.Providers.TItemModel));
            broker.RegisterTask( 
                "null", thisModule.UniqueName, TaskScheduler.IntervalType.isolatedThread, 0,  null, 
                "Host for web service[REST main service]");
        }
        public void IsolatedProducer(TItemModel parameters)
        {
            var appHost = new baseService();
            appHost.Init();
            appHost.Start(ListeningOn);

            System.Console.WriteLine("AppHost queue services Created at {0}, listening on {1}",
                DateTime.Now, ListeningOn);
        }
        public void Exit()
        {
        }
    }
}
