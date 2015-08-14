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
            LoggerWrapper.logger = logger;
            if (broker != null)
            {// except 
            }
            QueueService.ModProducer.broker = context;
            if (!System.Web.Hosting.HostingEnvironment.IsHosted)
            {
                appHost = new baseService();
                appHost.Init();
            }
            else
            {
                logger.Info("the code is hosted, can't start self hosted messaging service");
            }
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
    public class WrapperLogFactory : ServiceStack.Logging.ILogFactory
    {
        private StringBuilder sb;

        public WrapperLogFactory()
        {
            sb = new StringBuilder();
        }

        public ServiceStack.Logging.ILog GetLogger(Type type)
        {
            return new LoggerWrapper(type, sb);
        }

        public ServiceStack.Logging.ILog GetLogger(string typeName)
        {
            return new LoggerWrapper(typeName, sb);
        }

        public string GetLogs()
        {
            return null;
        }

        public void ClearLogs()
        {

        }
    }
    public class LoggerWrapper : ServiceStack.Logging.ILog
    {
        const string DEBUG = "DEBUG: ";
        const string ERROR = "ERROR: ";
        const string FATAL = "FATAL: ";
        const string INFO = "INFO: ";
        const string WARN = "WARN: ";
        //private readonly StringBuilder logs;
        public static ILogger logger;
        public LoggerWrapper(string type, StringBuilder logs)
        {

        }

        public LoggerWrapper(Type type, StringBuilder logs)
        {

        }

        public bool IsDebugEnabled { get { return true; } }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        private void Log(object message, Exception exception)
        {
            logger.Exception(exception, message.ToString(), "", "");
        }

        /// <summary>
        /// Logs the format.
        /// </summary>
        private void LogFormat(object message, params object[] args)
        {
            logger.Warning(message.ToString(), args);
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void Log(object message)
        {
            logger.Warning(message.ToString());
        }

        public void Debug(object message, Exception exception)
        {
            Log(DEBUG + message, exception);
        }

        public void Debug(object message)
        {
            Log(DEBUG + message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            LogFormat(DEBUG + format, args);
        }

        public void Error(object message, Exception exception)
        {
            Log(ERROR + message, exception);
        }

        public void Error(object message)
        {
            Log(ERROR + message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            LogFormat(ERROR + format, args);
        }

        public void Fatal(object message, Exception exception)
        {
            Log(FATAL + message, exception);
        }

        public void Fatal(object message)
        {
            Log(FATAL + message);
        }

        public void FatalFormat(string format, params object[] args)
        {
            LogFormat(FATAL + format, args);
        }

        public void Info(object message, Exception exception)
        {
            Log(INFO + message, exception);
        }

        public void Info(object message)
        {
            Log(INFO + message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            LogFormat(INFO + format, args);
        }

        public void Warn(object message, Exception exception)
        {
            Log(WARN + message, exception);
        }

        public void Warn(object message)
        {
            Log(WARN + message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            LogFormat(WARN + format, args);
        }
    }
}
