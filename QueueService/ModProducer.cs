using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueService
{
    public static class ModProducer
    {
        private const string ListeningOn = "http://localhost:82/";
        public static TaskBroker.Broker broker;

        public static void Initialise(TaskBroker.Broker brokerInterface)
        {
            broker = brokerInterface;

            //broker.Tasks.Add()
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
