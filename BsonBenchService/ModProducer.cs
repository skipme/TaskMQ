using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TaskQueue.Providers;
using TaskUniversum;
using TaskUniversum.Task;

namespace BsonBenchService
{
    public class ModProducer : IModIsolatedProducer
    {
        public const string ListeningOn = "0.0.0.0:83";// 5672

        public static IBroker broker;
        static TcpListener serv;
        ILogger logger;

        public class BsonClient
        {
            ILogger logger;
            public BsonClient(ILogger logger)
            {
                this.logger = logger;
                lastChecking = DateTime.UtcNow;
            }

            public TCPBsonBase.ServerClientCtx ctx;
            public TcpClient client;
            public NetworkStream ns;

            private DateTime lastChecking;
            public bool isAlive
            {
                get
                {
                    if ((DateTime.UtcNow - lastChecking).TotalSeconds > 10)
                    {
                        lastChecking = DateTime.UtcNow;

                        Socket s = client.Client;

                        bool blockingState = s.Blocking;
                        try
                        {
                            byte[] tmp = new byte[1];

                            s.Blocking = false;
                            s.Send(tmp, 0, 0);
                        }
                        catch (SocketException e)
                        {
                            // 10035 == WSAEWOULDBLOCK 
                            if (!e.NativeErrorCode.Equals(10035))
                            {
                                return false;
                            }
                            //if (e.NativeErrorCode.Equals(10035))
                            //    Console.WriteLine("Still Connected, but the Send would block");
                            //else
                            //{
                            //    Console.WriteLine("Disconnected: error code {0}!", e.NativeErrorCode);
                            //    return false;
                            //}
                        }
                        finally
                        {
                            s.Blocking = blockingState;
                        }

                    }
                    return true;
                }
            }

            public bool proc()
            {
                try
                {
                    ctx.ReadSelfState(ns);
                    ctx.ProcState();
                    ctx.WriteOppositeStateIfRequired(ns);
                }
                catch (Exception exc)
                {
                    logger.Exception(exc, "serve client", "while processing TcpClient");
                    return false;
                }
                return true;
            }
        }
        readonly object syncClients = new object();
        public readonly List<BsonClient> activeClients = new List<BsonClient>();

        public void Initialise(IBroker context, IBrokerModule thisModule)
        {
            logger = context.APILogger();
            broker = context;
        }
        public bool pushMessage(Dictionary<string, object> msg)
        {

            //           benchMessage
            TaskQueue.Providers.TaskMessage tm = new TaskQueue.Providers.TaskMessage(msg);
            //logger.Debug("put {0}", tm.MType);
            bool result = false;
            if (tm.MType != null)
            {
                try
                {
                    result = broker.PushMessage(tm);
                }
                catch (Exception e)
                {
                    logger.Exception(e, "message put", "error while message processing");
                }
            }
            return result;
        }
        public void IsolatedProducer(Dictionary<string, object> parameters)
        {
            //TCPBsonBase.ServerClientCtx ctx = new TCPBsonBase.ServerClientCtx();

            try
            {
                string[] paddr = ListeningOn.Split(':');
                serv = new TcpListener(IPAddress.Any, Int32.Parse(paddr[1]));
                serv.Start();
                logger.Info("TCP queue services Created at {0}, listening on {1}", DateTime.Now, ListeningOn);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (e.Message == "Access denied")
                {
                    logger.Exception(e, "http service start listening", "check permissions/firewall parameters...");
                }
                throw;
            }

            serv.BeginAcceptTcpClient(AcceptNewClient, null);
            int ringIndex = 0;
            while (!stopSignal)
            {
                if (activeClients.Count > 0)
                {
                    BsonClient cli = null;
                    lock (syncClients)
                        cli = activeClients[ringIndex];

                    if (cli.ns.DataAvailable)
                    {
                        cli.proc();
                    }
                    else
                    {
                        if (!cli.isAlive)
                        {
                            lock (syncClients)
                                activeClients.RemoveAt(ringIndex);
                            ringIndex--;
                            logger.Debug("Disconnected: {0}", cli.client.Client.RemoteEndPoint);
                        }
                        System.Threading.Thread.Sleep(0);
                    }

                    ringIndex++;
                    if (ringIndex >= activeClients.Count)
                        ringIndex = 0;
                }
                else
                {
                    System.Threading.Thread.Sleep(500);
                }
            }

        }

        private void AcceptNewClient(IAsyncResult s)
        {
            TcpClient tc = serv.EndAcceptTcpClient(s);
            BsonClient cli = new BsonClient(logger)
            {
                ctx = new TCPBsonBase.ServerClientCtx(),
                client = tc,
                ns = tc.GetStream()
            };
            cli.ns.ReadTimeout = 10;
            cli.ctx.PutCallback = pushMessage;
            lock (syncClients)
                activeClients.Add(cli);
            logger.Debug("Connected: {0}", tc.Client.RemoteEndPoint);

            serv.BeginAcceptTcpClient(AcceptNewClient, null);
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
                NameAndDescription = "Host for BSON service (BSON over TCP)"

            };
            return new MetaTask[] { t };
        }


        public string Name
        {
            get { return "BSON-service"; }
        }

        public string Description
        {
            get { return ""; }
        }

    }

}
