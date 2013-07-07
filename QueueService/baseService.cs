using Funq;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.WebHost.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueService
{
    //[Route("/queue")]
    //public class Queue : IReturn<QueueResponse>
    //{
    //    public string Name { get; set; }
    //    public string Description { get; set; }
    //}
    //public class QueueResponse
    //{
    //    public List<Queue> Result { get; set; }
    //}

    //[Route("/channels")]
    //public class MessageType : IReturn<MessageTypeResponse>
    //{
    //    public string Name { get; set; }
    //    public string Connection { get; set; }
    //    public string QueueName { get; set; }
    //}
    //public class MessageTypeResponse
    //{
    //    public List<MessageType> Result { get; set; }
    //}

    //[Route("/tasks")]
    //public class BrokerTasks : IReturn<BrokerTasksResponse>
    //{
    //    public string Name { get; set; }
    //    public string Description { get; set; }
    //}
    //public class BrokerTasksResponse
    //{
    //    public List<BrokerTasks> Result { get; set; }
    //}
    //[Route("/messages")]
    //public class Message : IReturn<MessageResponse>
    //{
    //    //public TaskQueue.Providers.TaskMessage Body { get; set; }
    //    public Dictionary<string, object> Body { get; set; }
    //}
    //public class MessageResponse
    //{
    //    public string Result { get; set; }
    //    public Message ResultMessage { get; set; }
    //}
    //public class qService : IService
    //{
    //    public QueueResponse Get(Queue request)
    //    {
    //        //TaskBroker.Broker b = ModProducer.broker;
    //        //QueueResponse r = new QueueResponse();
    //        //r.Result = new List<Queue>();
    //        //foreach (var q in b.MessageChannels.Queues.QueueList.Values)
    //        //{
    //        //    r.Result.Add(new Queue()
    //        //    {
    //        //        Name = q.QueueType,
    //        //        Description = q.QueueDescription
    //        //    });
    //        //}
    //        //return r;
    //        return null;
    //    }
    //    public MessageTypeResponse Get(MessageType request)
    //    {
    //        //TaskBroker.Broker b = ModProducer.broker;
    //        //MessageTypeResponse r = new MessageTypeResponse();
    //        //r.Result = new List<MessageType>();
    //        //foreach (var s in b.MessageChannels.MessageModels)
    //        //{
    //        //    r.Result.Add(new MessageType()
    //        //    {
    //        //        Name = s.UniqueName
    //        //    });
    //        //}
    //        //return r;
    //        return null;
    //    }
    //    public BrokerTasksResponse Get(BrokerTasks request)
    //    {
    //        //TaskBroker.Broker b = ModProducer.broker;
    //        //BrokerTasksResponse r = new BrokerTasksResponse();
    //        //r.Result = new List<BrokerTasks>();
    //        //foreach (var s in b.Tasks)
    //        //{
    //        //    r.Result.Add(new BrokerTasks()
    //        //    {
    //        //        Name = s.Name,
    //        //        Description = s.Description
    //        //    });
    //        //}
    //        //return r;
    //        return null;
    //    }
    //    public MessageResponse Post(Message request)
    //    {
    //        //TaskBroker.Broker b = ModProducer.broker;
    //        //TaskQueue.Providers.TaskMessage m = new TaskQueue.Providers.TaskMessage(request.Body);
    //        //TaskQueue.Providers.TaskMessage mp = b.Pop(m.MType);
    //        //return new MessageResponse() { Result = mp == null ? "Empty" : "OK", ResultMessage = new Message() { Body = mp == null ? null : mp.GetHolder() } };
    //        return null;
    //        return null;
    //    }
    //    public MessageResponse Put(Message request)
    //    {
    //        //TaskBroker.Broker b = ModProducer.broker;
    //        //TaskQueue.Providers.TaskMessage msg = new TaskQueue.Providers.TaskMessage(request.Body);
    //        //bool result = b.PushMessage(msg);

    //        //return new MessageResponse() { Result = result ? "OK" : "FAIL", ResultMessage = new Message() { Body = request.Body } };
    //        return null;
    //    }
    //}
    public class ngService : RestServiceBase<Dictionary<string, object>>
    {
        public override object OnPut(Dictionary<string, object> m)
        {
            // save to db
            TaskQueue.Providers.TaskMessage tm = new TaskQueue.Providers.TaskMessage(m);
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
              .Add<Dictionary<string, object>>("/q", "GET,PUT");
        }
    }
}
