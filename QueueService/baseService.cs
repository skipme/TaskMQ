using Funq;
using ServiceStack.ServiceHost;
using ServiceStack.WebHost.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueService
{
    [Route("/queue")]
    public class Queue : IReturn<QueueResponse>
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class QueueResponse
    {
        public List<Queue> Result { get; set; }
    }
    [Route("/messages")]
    public class MessageType : IReturn<MessageTypeResponse>
    {
        public string Name { get; set; }
    }
    public class MessageTypeResponse
    {
        public List<MessageType> Result { get; set; }
    }
    public class qService : IService
    {
        public QueueResponse Get(Queue request)
        {
            TaskBroker.Broker b = ModProducer.broker;
            QueueResponse r = new QueueResponse();
            r.Result = new List<Queue>();
            foreach (var q in b.Queues.QueueList.Values)
            {
                r.Result.Add(new Queue()
                {
                    Name = q.QueueType,
                    Description = q.QueueDescription
                });
            }
            return r;
        }
        public MessageTypeResponse Get(MessageType request)
        {
            TaskBroker.Broker b = ModProducer.broker;
            MessageTypeResponse r = new MessageTypeResponse();
            r.Result = new List<MessageType>();
            foreach (var s in b.MessageSchemas)
            {
                r.Result.Add(new MessageType()
                {
                    Name = s.Name
                });
            }
            return r;
        }
    }
    public class baseService : AppHostHttpListenerBase
    {
        /// <summary>
        /// Initializes a new instance of your ServiceStack application, with the specified name and assembly containing the services.
        /// </summary>
        public baseService() : base("QueueServices", typeof(qService).Assembly) { }

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
        }
    }
}
