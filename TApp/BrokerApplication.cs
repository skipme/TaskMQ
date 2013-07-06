using MongoQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using TaskScheduler;

namespace TaskBroker
{
    [SecurityPermission(SecurityAction.Demand, Infrastructure = true)]
    public class BrokerApplication : MarshalByRefObject, IDisposable
    {
        TaskBroker.Broker broker;
        public void Dispose()
        {
        }

        public void Run(ManualResetEvent signal)
        {
            broker = new Broker();
            broker.AddAssemblyByPath("QueueService.dll");
            broker.RegisterConnection<MongoDbQueue>("MongoLocalhost",
               "mongodb://user:1234@localhost:27017/?safe=true", "Messages", "TaskMQ");
            broker.RegisterConnection<MongoDbQueue>("MongoLocalhostEmail",
    "mongodb://user:1234@localhost:27017/?safe=true", "Messages", "email");
            broker.RegisterChannel<EmailConsumer.MailModel>("MongoLocalhostEmail", "EmailC");
            EmailConsumer.SmtpModel smtp = new EmailConsumer.SmtpModel()
            {
                Login = "user",
                UseSSL = true,
                Port = 587,
                Password = "",
                Server = "smtp.yandex.ru"
            };
            broker.ReloadModules();
            broker.RegisterTask(
                "EmailC", "EmailSender",
                IntervalType.intervalMilliseconds, 1000, smtp,
                "Email Common channel consumer on mongo db queue channel");

            broker.StopBroker(); // ! stop scheduler and other isolated threads
            signal.Set();
        }
    }
}
