using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TaskBroker;


namespace TApp
{
    class Program
    {
        //netsh http add urlacl url=http://+:82/ user=User
        static void RestartAsAdmin()
        {
            var startInfo = new ProcessStartInfo("TApp.exe") { Verb = "runas" };
            Process.Start(startInfo);
            Environment.Exit(0);
        }
        class zModel : TaskQueue.Providers.TaskMessage
        {
            public const string Name = "z";
            public zModel() : base(Name) { }
            public string SomeProperty { get; set; }
        }
        class zConsumer : IModConsumer
        {
            public bool Push(TaskQueue.Providers.TItemModel parameters, ref TaskQueue.Providers.TaskMessage q_parameter)
            {
                Console.WriteLine("zMessage: {0} - {1} {2}", q_parameter.GetHolder()["MType"], q_parameter.AddedTime, q_parameter.GetHolder()["_id"]);
                return true;
            }

            public void Exit()
            {

            }

            public void Initialise(Broker brokerInterface, ModMod thisModule)
            {

            }
        }
        static void Main(string[] args)
        {
            var prefix = QueueService.ModProducer.ListeningOn;
            var username = Environment.GetEnvironmentVariable("USERNAME");
            var userdomain = Environment.GetEnvironmentVariable("USERDOMAIN");
            Console.WriteLine("  netsh http add urlacl url={0} user={1}\\{2} listen=yes",
                    prefix, userdomain, username);


            TaskQueue.QueueItemModel tm = new TaskQueue.QueueItemModel(typeof(zModel));
            MSSQLQueue.SqlTable t = new MSSQLQueue.SqlTable(tm, "Z");
            string rst = MSSQLQueue.SqlScript.ForTableGen(t);
            //
            TaskBroker.Broker b = new TaskBroker.Broker();

            //
            b.AddConnection(new TaskQueue.Providers.QueueConnectionParameters()
                {
                    Collection = "TaskMQ",
                    ConnectionString = "mongodb://user:1234@localhost:27017/?safe=true",//db.addUser('user','1234')
                    Database = "Messages",
                    Name = "MongoLocalhost"
                });
            //
            b.RegistarateMessageModel(new TaskBroker.MessageType(new zModel()));
            b.RegistarateChannel(new TaskBroker.MessageChannel()
                {
                    QueueName = "MongoDBQ",
                    ConnectionParameters = "MongoLocalhost",
                    UniqueName = "z"
                }, zModel.Name);
            //
            // web service
            //TaskBroker.ModMod m = new TaskBroker.ModMod()
            //{
            //    InitialiseEntry = QueueService.ModProducer.Initialise,
            //    ExitEntry = QueueService.ModProducer.Exit,
            //    ModAssembly = typeof(QueueService.ModProducer).Assembly,
            //};
            //b.RegistrateModule(m);

            //
            // z channel consumer ...
            //TaskBroker.ModMod mcZ = new TaskBroker.ModMod()
            //{
            //    InitialiseEntry = (TaskBroker.Broker bb, TaskBroker.ModMod mm) => { },
            //    ExitEntry = () => { },
            //    ModAssembly = typeof(zConsumer).Assembly,
            //    Consumer = zConsumer.Entry,
            //    AcceptsModel = new TaskQueue.QueueItemModel(typeof(zModel)),
            //    InvokeAs = TaskBroker.ExecutionType.Consumer,
            //    UniqueName = "zConsumer",

            //};
            //b.RegistrateModule(mcZ);
            b.RegistrateCosumerModule<zConsumer, zModel>("ZConsume");

            //
            //TaskBroker.ModMod email = new TaskBroker.ModMod()
            //{
            //    InitialiseEntry = EmailConsumer.ModConsumer.Initialise,

            //};
            //b.RegistrateModule(email);

            //
            //b.RegistrateTask("zConsumer - default", "z", "zConsumer", " zConsumer no desc", 
            //    TaskScheduler.IntervalType.everyCustomMilliseconds, 1000);

            //

            //
            b.AddConnection(new TaskQueue.Providers.QueueConnectionParameters()
            {
                Collection = "email",
                ConnectionString = "mongodb://user:1234@localhost:27017/?safe=true",//db.addUser('user','1234')
                Database = "Messages",
                Name = "MongoLocalhostEmail"
            });

            TaskBroker.MessageChannel emailChannel = new TaskBroker.MessageChannel("EmailC", "MongoLocalhostEmail", "MongoDBQ");
            b.RegistarateChannel(emailChannel, EmailConsumer.MailModel.Name);
            EmailConsumer.SmtpModel smtp = new EmailConsumer.SmtpModel()
            {
                Login = "user",
                UseSSL = true,
                Port = 587,
                Password = "",
                Server = "smtp.yandex.ru"
            };
            b.RegistrateTask("Email Common", emailChannel.UniqueName, "EmailSender", "Email Common channel consumer",
                TaskScheduler.IntervalType.everyCustomMilliseconds, 1000, smtp);

            //

            Console.Read();
            b.Scheduler.SuspendAll();
        }
    }
}
