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
        public bool MainPart { get; set; }
        public bool ModulesPart { get; set; }
        public bool AssemblysPart { get; set; }

        public string Body { get; set; }
        public string ConfigId { get; set; }
    }
    [Route("/tmq/c/commit", Verbs = "POST")]
    public class ConfigCommitRequest
    {
        public bool Reset { get; set; }
        public bool Restart { get; set; }

        // IDS:
        public string MainPart { get; set; }
        public string ModulesPart { get; set; }
        public string AssemblysPart { get; set; }
    }
    [Route("/tmq/s", Verbs = "GET")]
    public class StatisticRequest
    {
        public bool GetHeartbit { get; set; }
    }

    public class ItemCounter
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public string Left { get; set; }
    }
    public class StatisticResponseHeartbit
    {
        public List<ItemCounter> Channels { get; set; }
    }

    public class ConfigResponse
    {
        public string ConfigCommitID { get; set; }
        public string Result { get; set; }
    }
    [ClientCanSwapTemplates]
    public class ngService : Service
    {
        public object Get(StatisticRequest r)
        {

            StatisticResponseHeartbit h = new StatisticResponseHeartbit();
            h.Channels = new List<ItemCounter>();
            foreach (TaskBroker.Statistics.BrokerStat sm in QueueService.ModProducer.broker.MessageChannels.GetStatistics())
            {
                TaskBroker.Statistics.StatRange range = sm.GetFlushedMinRange();
                h.Channels.Add(new ItemCounter()
                {
                    Name = sm.Name,
                    Count = (int)range.PerMinute,
                    Left = range.Left.ToString()
                });
            }

            return h;
        }
        public object Put(Dictionary<string, object> m)
        {
            // save to db
            TaskQueue.Providers.TaskMessage tm = new TaskQueue.Providers.TaskMessage(m);
            bool result = false;
            if (tm.MType != null)
                result = QueueService.ModProducer.broker.PushMessage(tm);
            return new ServiceStack.Common.Web.HttpResult()
            {
                StatusCode = result ? HttpStatusCode.Created : HttpStatusCode.InternalServerError
            };
        }
        public object Get(ConfigRequest request)
        {
            if (request.MainPart)
                return TaskBroker.Configuration.BrokerConfiguration.ExtractFromBroker(QueueService.ModProducer.broker).Serialise();
            else if (request.ModulesPart)
                return TaskBroker.Configuration.BrokerConfiguration.ExtractModulesFromBroker(QueueService.ModProducer.broker).Serialise();
            return null;
        }
        public ConfigResponse Post(ConfigRequest request)
        {
            if (request.MainPart)
            {
                QueueService.ModProducer.broker.Configurations.RegisterConfiguration(request.ConfigId, request.Body);
                //Console.Write("conf main part set: {0}", request.Body);
            }
            else if (request.ModulesPart)
            {
                //Console.Write("conf mod part set: {0}", request.Body);
            }
            return new ConfigResponse()
            {
                Result = "OK", // OR SOME ERROR DESCRIPTION
                ConfigCommitID = request.ConfigId
            };
        }
        public ConfigResponse Post(ConfigCommitRequest request)
        {
            Console.WriteLine("id's {0} {1}", request.MainPart, request.ModulesPart);
            string errors = "";
            bool resp = false;

            if (request.MainPart != null)
                resp = QueueService.ModProducer.broker.Configurations.ValidateAndCommitMain(request.MainPart, out errors);

            if (resp && request.ModulesPart != null)
                resp = QueueService.ModProducer.broker.Configurations.ValidateAndCommitMods(request.ModulesPart, out errors);

            if (resp)
            {
                errors = "OK";
                if (request.Reset)
                {
                    // delay reset....
                }
                else if (request.Restart)
                {
                    // delay restart....
                }
            }
            else
            {
                return new ConfigResponse()
                {
                    Result = errors,
                    ConfigCommitID = null
                };
            }
            return new ConfigResponse()
            {
                Result = errors, // OR SOME ERROR DESCRIPTION
                ConfigCommitID = null
            };
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
