using Funq;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.WebHost.Endpoints;
using System;
using System.Collections.Generic;
using System.Net;

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
        public bool GetThroughput { get; set; }
    }
    [Route("/tmq/v", Verbs = "POST")]
    public class ValidationRequest
    {
        public string MType { get; set; }
        public string ChannelName { get; set; }
    }
    public class ValidationResponse
    {
        public Dictionary<string, TaskQueue.RepresentedModelValue> ModelScheme { get; set; }
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
        string _lock = "lock";

        public object Get(StatisticRequest r)
        {
            StatisticResponseHeartbit h = new StatisticResponseHeartbit();
            h.Channels = new List<ItemCounter>();
            foreach (TaskBroker.Statistics.BrokerStat sm in QueueService.ModProducer.broker.MessageChannels.GetStatistics())
            {
                if (sm == null)// TODO: means channel not owns by any task
                    continue;
                TaskBroker.Statistics.StatRange range = sm.GetFlushedMinRange();
                h.Channels.Add(new ItemCounter()
                {
                    Name = sm.Name,
                    Count = r.GetThroughput ? (int)range.PerMinute : (int)range.Counter,
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
            //result = true;
            if (tm.MType != null)
            {
                try
                {
                    result = QueueService.ModProducer.broker.PushMessage(tm);
                }
                catch (Exception e)
                {
                    Console.WriteLine("error while message processing: {0} '{1}'", e.Message, e.StackTrace);
                    return new ServiceStack.Common.Web.HttpResult()
                    {
                        StatusCode = HttpStatusCode.InternalServerError
                    };
                }
            }
            return new ServiceStack.Common.Web.HttpResult()
            {
                StatusCode = result ? HttpStatusCode.Created :
                HttpStatusCode.NotAcceptable
            };
        }
        public object Get(ConfigRequest request)
        {
            if (request.MainPart)
                return TaskBroker.Configuration.BrokerConfiguration.ExtractFromBroker(QueueService.ModProducer.broker).SerialiseJson();
            else if (request.ModulesPart)
                return TaskBroker.Configuration.BrokerConfiguration.ExtractModulesFromBroker(QueueService.ModProducer.broker).SerialiseJson();
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

            lock (_lock)
            {
                if (request.MainPart != null)
                    resp = QueueService.ModProducer.broker.Configurations.ValidateAndCommitMain(request.MainPart, out errors);

                if (resp && request.ModulesPart != null)
                    resp = QueueService.ModProducer.broker.Configurations.ValidateAndCommitMods(request.ModulesPart, out errors);
            }
            if (resp)
            {
                errors = "OK";
                if (request.Reset)
                {
                    // delay reset....
                    QueueService.ModProducer.broker.resetBroker();
                }
                else if (request.Restart)
                {
                    // delay restart....
                    QueueService.ModProducer.broker.restartApp();
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
        public object Post(ValidationRequest request)
        {
            TaskQueue.RepresentedModel model;
            if (request.MType == null)
                return new ServiceStack.Common.Web.HttpResult()
                {
                    StatusCode = HttpStatusCode.NotAcceptable
                };
            else if (request.ChannelName == null)
            {
                // MType
                model = QueueService.ModProducer.broker.GetValidationModel(request.MType);
            }
            else
            {
                // channel + MType
                model = QueueService.ModProducer.broker.GetValidationModel(request.MType, request.ChannelName);
            }
            if (model == null)
                return new ServiceStack.Common.Web.HttpResult()
                {
                    StatusCode = HttpStatusCode.NotAcceptable
                };
            return new ValidationResponse()
            {
                ModelScheme = model.schema.ToDictionary()
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
                //WsdlServiceNamespace = "http://localhost:82/"
            });

            Routes
              .Add<Dictionary<string, object>>("/tmq/q", "GET,PUT");
        }
    }
}
