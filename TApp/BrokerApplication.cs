using MongoQueue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using TaskBroker.Configuration;
using TaskScheduler;

namespace TaskBroker
{
    [SecurityPermission(SecurityAction.Demand, Infrastructure = true)]
    public class BrokerApplication : MarshalByRefObject, IDisposable
    {
        ManualResetEvent Signal;

        TaskBroker.Broker broker;
        public void Dispose()
        {
        }
        void Restart()
        {
            broker.StopBroker(); // ! stop scheduler and other isolated threads
            Signal.Set();
        }
        public void Run(ManualResetEvent signal)
        {
            broker = new Broker(Restart);
            //QueueService.ModProducer m;// todo: force loading local dep's
            //EmailConsumer.ModConsumer ec;


            //broker.AddAssemblyByPath("QueueService.dll");
    //        broker.RegisterConnection<MongoDbQueue>("MongoLocalhost",
    //           "mongodb://user:1234@localhost:27017/?safe=true", "Messages", "TaskMQ");
    //        broker.RegisterConnection<MongoDbQueue>("MongoLocalhostEmail",
    //"mongodb://user:1234@localhost:27017/?safe=true", "Messages", "email");
    //broker.RegisterChannel<EmailConsumer.MailModel>("MongoLocalhostEmail", "EmailC");
    //        EmailConsumer.SmtpModel smtp = new EmailConsumer.SmtpModel()
    //        {
    //            Login = "usr",
    //            UseSSL = true,
    //            Port = 587,
    //            Password = "pwd",
    //            Server = "smtp.yandex.ru"
    //        };

    //        broker.LoadAssemblys();

    //        broker.RegisterTask(
    //            "EmailC", "EmailSender",
    //            //IntervalType.intervalMilliseconds, 1000, smtp,
    //            IntervalType.withoutInterval, 0, smtp.ToDictionary(),
    //            "Email Common channel consumer on mongo db queue channel");
            broker.LoadAssemblys();
            broker.RevokeBroker(true);
            this.Signal = signal;

            exportConfiguration();
        }

        public void exportConfiguration()
        {
            File.WriteAllBytes("main.json", BrokerConfiguration.ExtractFromBroker(broker).SerialiseJson());
            File.WriteAllBytes("modules.json", BrokerConfiguration.ExtractModulesFromBroker(broker).SerialiseJson());
            File.WriteAllBytes("assemblys.json", BrokerConfiguration.ExtractAssemblysFromBroker(broker).SerialiseJson());
        }
    }
}
