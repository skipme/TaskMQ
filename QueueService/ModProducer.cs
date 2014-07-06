using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;
using TaskUniversum;
using TaskUniversum.Task;

namespace QueueService
{
    public class ModProducer : IModIsolatedProducer
    {
        public const string ListeningOn = "http://+:82/";
        baseService appHost;
        public static IBroker broker;

        ILogger logger;

        public void Initialise(IBroker context, IBrokerModule thisModule)
        {
            logger = context.APILogger();
            if (broker != null)
            {// except 
            }
            QueueService.ModProducer.broker = context;

            appHost = new baseService();
            appHost.Init();
        }
        public void IsolatedProducer(Dictionary<string, object> parameters)
        {
            try
            {
                appHost.Start(ListeningOn);
                logger.Info("AppHost queue services Created at {0}, listening on {1}", DateTime.Now, ListeningOn);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (e.Message == "Access denied")
                {
                    logger.Exception(e, "http service start listening", "check permissions/firewall parameters...");
                }
                throw e;
            }
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

        public MetaTask[] RegisterTasks(IBrokerModule thisModule)
        {
            MetaTask t = new MetaTask()
            {
                intervalType = IntervalType.isolatedThread,
                ModuleName = thisModule.UniqueName,
                NameAndDescription = "Host for web service[REST main service]"

            };
            return new MetaTask[] { t };
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
