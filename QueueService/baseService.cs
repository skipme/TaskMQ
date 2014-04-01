using Funq;
using Newtonsoft.Json;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Net;
using TaskUniversum.Statistics;

namespace QueueService
{
    [Route("/tmq/c", Verbs = "GET,POST")]
    public class ConfigRequest
    {
        public bool MainPart { get; set; }
        public bool ModulesPart { get; set; }
        public bool AssemblysPart { get; set; }

        public bool ConfigurationExtra { get; set; } // registered message queue types, build server types

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

    [Route("/tmq/assemblies", Verbs = "GET")]
    public class AssembliesRequest
    {
        public bool Statuses { get; set; }
        public bool UpToDateAll { get; set; }
        public bool Fetch { get; set; }
        public bool Build { get; set; }
        public bool Package { get; set; }

        public bool CheckBS { get; set; }
        public string BSName { get; set; }
        public string BSParameters { get; set; }

        public string Name { get; set; }
    }

    public class AssemblyStatus
    {
        public string Name { get; set; }
        public string state { get; set; }
        public string revisionTag { get; set; }
        public string revisionSourceTag { get; set; }

        public string revCommiter { get; set; }
        public string revCommitComment { get; set; }
        public DateTime revCommitTime { get; set; }

        public string revSCommiter { get; set; }
        public string revSCommitComment { get; set; }
        public DateTime revSCommitTime { get; set; }

        public DateTime packaged { get; set; }
        public bool loaded { get; set; }
        public string loadedRevision { get; set; }
        public string loadedRemarks { get; set; }
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
        public object Get(AssembliesRequest r)
        {
            TaskUniversum.ISourceManager sourceManager = QueueService.ModProducer.broker.GetSourceManager();
            string _na = "not available";
            if (r.Statuses)
            {
                lock (_lock)
                {
                    List<AssemblyStatus> resp = new List<AssemblyStatus>();
                    foreach (var asm in QueueService.ModProducer.broker.GetSourceStatuses())
                    {
                        AssemblyStatus expStat = new AssemblyStatus
                        {
                            Name = asm.Key,
                            state = asm.Value.State,
                            revisionTag = asm.Value.PackageRev.Revision,

                            loaded = asm.Value.Loaded,
                            packaged = asm.Value.packagedDate,
                            loadedRevision = asm.Value.LoadedRevision,
                            loadedRemarks = asm.Value.LoadedRemarks,

                            revCommitComment = asm.Value.PackageRev.CommitMessage,
                            revCommiter = asm.Value.PackageRev.Commiter,
                            revCommitTime = asm.Value.PackageRev.CommitTime,

                            revisionSourceTag = _na,
                            revSCommitComment = _na,
                            revSCommiter = _na,
                            revSCommitTime = DateTime.MinValue
                        };
                        if (asm.Value.BuildServerRev != null)
                        {
                            expStat.revisionSourceTag = asm.Value.BuildServerRev.Revision;
                            expStat.revSCommitComment = asm.Value.BuildServerRev.CommitMessage;
                            expStat.revSCommiter = asm.Value.BuildServerRev.Commiter;
                            expStat.revSCommitTime = asm.Value.BuildServerRev.CommitTime;
                        }
                        resp.Add(expStat);
                    }
                    return resp;
                }
            }
            else if (r.Build)
            {
                sourceManager.BuildSource(r.Name);
            }
            else if (r.Fetch)
            {
                sourceManager.FetchSource(r.Name);
            }
            else if (r.Package)
            {
                sourceManager.UpdatePackage(r.Name);
            }
            else if (r.CheckBS)
            {
                Dictionary<string, object> bsParameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(r.BSParameters);
                string explain = null;
                bool result = sourceManager.CheckBuildServerParameters(r.BSName, bsParameters, out explain);
                return new
                {
                    CheckResult = result,
                    Remark = explain
                };
            }
            return new ServiceStack.HttpResult()
            {
                StatusCode = HttpStatusCode.OK
            };
        }
        public object Get(StatisticRequest r)
        {
            StatisticResponseHeartbit h = new StatisticResponseHeartbit();
            h.Channels = new List<ItemCounter>();

            StatisticContainer statCont = QueueService.ModProducer.broker.GetChannelsStatistic();
            foreach (MetaStatRange mrange in statCont.FlushedMinRanges)
            {
                StatRange range = mrange.range;
                h.Channels.Add(new ItemCounter()
                {
                    Name = mrange.Name,
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
                    return new ServiceStack.HttpResult()
                    {
                        StatusCode = HttpStatusCode.InternalServerError
                    };
                }
            }
            return new ServiceStack.HttpResult()
            {
                StatusCode = result ? HttpStatusCode.Created :
                HttpStatusCode.NotAcceptable
            };
        }
        public object Get(ConfigRequest request)
        {
            return QueueService.ModProducer.broker.GetCurrentConfiguration(request.MainPart, request.ModulesPart, request.AssemblysPart, request.ConfigurationExtra);
            //if (request.MainPart)
            //    return TaskBroker.Configuration.BrokerConfiguration.ExtractFromBroker(QueueService.ModProducer.broker).SerialiseJson();
            //else if (request.ModulesPart)
            //    return TaskBroker.Configuration.BrokerConfiguration.ExtractModulesFromBroker(QueueService.ModProducer.broker).SerialiseJson();
            //else if (request.AssemblysPart)
            //    return TaskBroker.Configuration.BrokerConfiguration.ExtractAssemblysFromBroker(QueueService.ModProducer.broker).SerialiseJson();
            //else if (request.ConfigurationExtra)
            //{
            //    return QueueService.ModProducer.broker.GetSourceManager().GetJsonBuildServersConfiguration();
            //}
            //return null;
        }
        public ConfigResponse Post(ConfigRequest request)
        {
            if (request.MainPart)
            {
                QueueService.ModProducer.broker.RegisterNewConfiguration(request.ConfigId, request.Body);
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

            resp = QueueService.ModProducer.broker.ValidateAndCommitConfigurations(request.MainPart, request.ModulesPart, out errors, request.Reset, request.Restart);
            if (resp)
            {
                errors = "OK";
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
                return new ServiceStack.HttpResult()
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
                return new ServiceStack.HttpResult()
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
            //base.SetConfig(new EndpointHostConfig
            //{
            //    GlobalResponseHeaders =
            //    {
            //        { "Access-Control-Allow-Origin", "*" },
            //        { "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
            //    },
            //    //WsdlServiceNamespace = "http://localhost:82/"
            //});

            Routes
              .Add<Dictionary<string, object>>("/tmq/q", "GET,PUT");
            ServiceStack.Text.JsConfig.DateHandler = ServiceStack.Text.DateHandler.ISO8601;
        }
    }
}
