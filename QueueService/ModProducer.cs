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
        baseService appHost = new baseService();

        public void Initialise(TaskBroker.Broker brokerInterface, TaskBroker.ModMod thisModule)
        {
            broker = brokerInterface;
            //thisModule.Producer = IsolatedProducer;
            thisModule.MI = this;
            //thisModule.ParametersModel = new TaskQueue.QueueItemModel(typeof(TaskQueue.Providers.TItemModel));

        }
        public void IsolatedProducer(TItemModel parameters)
        {
            //appHost = new baseService();
            appHost.Init();
            appHost.Start(ListeningOn);

            System.Console.WriteLine("AppHost queue services Created at {0}, listening on {1}",
                DateTime.Now, ListeningOn);
        }
        public void IsolatedProducerStop()
        {
            if (appHost != null)
            {
                appHost.Stop();
                appHost.Dispose();
                appHost = null;
            }
        }

        public void Exit()
        {
            
        }

        public QueueTask[] RegisterTasks(ModMod thisModule)
        {
            QueueTask t = new QueueTask()
            {
                intervalType = TaskScheduler.IntervalType.isolatedThread,
                Description = "",
                ModuleName = thisModule.UniqueName,
                NameAndDescription = "Host for web service[REST main service]"

            };
            return new QueueTask[] { t };
            //broker.RegisterTask(
            //     "null", thisModule.UniqueName, TaskScheduler.IntervalType.isolatedThread, 0, null,
            //     "Host for web service[REST main service]");
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
