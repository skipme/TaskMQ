using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TaskQueue.Providers;
using TaskUniversum;
using TaskUniversum.Task;

namespace RabbitMqService
{
    public class ModProducer : IModIsolatedProducer
    {
        public const string ListeningOn = "0.0.0.0:83";// 5672

        public static IBroker broker;

        ILogger logger;


        public void Initialise(IBroker context, IBrokerModule thisModule)
        {
            logger = context.APILogger();

        }
        public void IsolatedProducer(Dictionary<string, object> parameters)
        {
            try
            {

                //appHost.Start(ListeningOn);
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

            string[] paddr = ListeningOn.Split(':');

            TcpListener serv = new TcpListener(IPAddress.Any, Int32.Parse(paddr[1]));
            serv.Start();
            while (!stopSignal)
            {
                TcpClient cli = serv.AcceptTcpClient();
                
                NetworkStream ns = cli.GetStream();
                while (true)
                {
                    if (ns.DataAvailable)
                    {
                        // client sets state
                    }
                }
            }

        }
        volatile bool stopSignal = false;
        public void IsolatedProducerStop()
        {
            stopSignal = true;
            //if (appHost != null)
            {
                //appHost.Stop();
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
                NameAndDescription = "Host for AMQP service[RabbitMQ AMQP compatiable main service]"

            };
            return new MetaTask[] { t };
        }


        public string Name
        {
            get { return "AMQP-service"; }
        }

        public string Description
        {
            get { return ""; }
        }

    }

}
