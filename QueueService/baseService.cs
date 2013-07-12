using Funq;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.WebHost.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace QueueService
{
    [Route("/tmq/c", Verbs = "GET,POST")]
    public class ConfigRequest
    {
        public bool Reset { get; set; }
        public bool Restart { get; set; }

        public bool GetMain { get; set; }
        public bool GetModules { get; set; }
        public bool GetAssemblys { get; set; }

        public string Body { get; set; }
    }

    [ClientCanSwapTemplates]
    public class ngService : Service
    {
        public object Put(Dictionary<string, object> m)
        {
            // save to db
            TaskQueue.Providers.TaskMessage tm = new TaskQueue.Providers.TaskMessage(m);
            bool result = QueueService.ModProducer.broker.PushMessage(tm);
            return new ServiceStack.Common.Web.HttpResult()
            {
                StatusCode = result ? HttpStatusCode.Created : HttpStatusCode.InternalServerError
            };
        }
        public object Get(ConfigRequest request)
        {
            if (request.GetMain)
            return TaskBroker.Configuration.BrokerConfiguration.ExtractFromBroker(QueueService.ModProducer.broker).Serialise();
            else if (request.GetModules)
                return TaskBroker.Configuration.BrokerConfiguration.ExtractModulesFromBroker(QueueService.ModProducer.broker).Serialise();
            return null;
        }
    }
    public class baseService : AppHostHttpListenerBase
    {
        /// <summary>
        /// Initializes a new instance of your ServiceStack application, with the specified name and assembly containing the services.
        /// </summary>
        public baseService() : base("tmq", typeof(ngService).Assembly) { }

        /// <summary>
        /// Configure the container with the necessary routes for your ServiceStack application.
        /// </summary>
        /// <param name="container">The built-in IoC used with ServiceStack.</param>
        public override void Configure(Container container)
        {
            base.SetConfig(new EndpointHostConfig
            {
                GlobalResponseHeaders =
				{
					{ "Access-Control-Allow-Origin", "*" },
					{ "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
				},
                WsdlServiceNamespace = "http://localhost:82/"
            });

            Routes
              .Add<Dictionary<string, object>>("/tmq/q", "GET,PUT");
        }
    }
}
