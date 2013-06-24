using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using TaskQueue.Providers;
using TaskScheduler;

namespace TaskBroker
{
    public class Broker
    {
        public Broker()
        {
            Tasks = new List<QueueTask>();
            Modules = new ModHolder();
            MessageChannels = new QueueMTClassificator();

            Scheduler = new TaskScheduler.ThreadPool();
            //Scheduler.Allocate();
        }

        //public QueueConParams Connections;
        public QueueMTClassificator MessageChannels;
        public List<QueueTask> Tasks;
        public ModHolder Modules;
        //public QueueClassificator Queues;
        public TaskScheduler.ThreadPool Scheduler;

        //modules
        //bunch[model, queue, +module] .... TODO: maybe bunch with channel better?
        public void RegisterModule(ModMod mod)
        {
            mod.InitialiseEntry(this, mod);
            Modules.AddMod(mod);
        }

        public void RegisterConsumerModule<C, M>(string name)
            where C : IModConsumer
            where M : TaskMessage
        {
            TaskBroker.ModMod stub = new TaskBroker.ModMod()
                {
                    ModAssembly = typeof(C).Assembly,
                    AcceptsModel = new TaskQueue.QueueItemModel(typeof(M)),
                    Role = TaskBroker.ExecutionType.Consumer,
                    UniqueName = name,
                    MI = Activator.CreateInstance<C>()
                };

            stub.InitialiseEntry(this, stub);
            Modules.AddMod(stub);
        }
        public void RegisterSelfValuedModule<C>()
            where C : IModConsumer
        {
            TaskBroker.ModMod stub = new TaskBroker.ModMod()
            {
                ModAssembly = typeof(C).Assembly,
                Role = TaskBroker.ExecutionType.Consumer,
                MI = Activator.CreateInstance<C>()
            };
            stub.InitialiseEntry(this, stub);
            Modules.AddMod(stub);
        }
        public void RegistrateModule(System.Reflection.Assembly mod)
        {
            Modules.AddMod(mod);
        }

        //public void RegisterMessageModel(MessageType mt)
        //{
        //    MessageChannels.AddMessageType(mt);
        //}
        //public void RegisterMessageModel<T>()
        //     where T : TaskQueue.Providers.TaskMessage
        //{
        //    TaskQueue.Providers.TaskMessage ta = Activator.CreateInstance<T>();
        //    MessageChannels.AddMessageType(new TaskBroker.MessageType(ta));
        //}


        //public void RegistarateChannel(MessageChannel mc, string messageModelName)
        //{
        //    MessageChannels.AddMessageChannel(mc, messageModelName);
        //}

        // channels 
        // bunch [model, queue, +channel]
        public void RegisterChannel<T>(string connectionName, string channelName)
            where T : TaskQueue.Providers.TItemModel
        {
            MessageChannels.AddMessageChannel<T>(new TaskBroker.MessageChannel()
            {
                ConnectionName = connectionName,
                UniqueName = channelName
            });
        }

        // bunch [channel, module, +executionContext]
        // note: this is configure which channel is selected for custom module
        public void RegisterTask(string Channel, string moduleName,
            IntervalType it = IntervalType.intervalMilliseconds,
            long intervalValue = 100,
            TItemModel parameters = null, string Description = "-")
        {
            ModMod module = Modules.GetByName(moduleName);

            if (module == null)
                throw new Exception("required qmodule not found.");

            QueueTask t = new QueueTask()
            {
                Module = module,
                Description = Description,
                ChannelName = Channel,
                Parameters = parameters,

                intervalType = it,
                intervalValue = intervalValue,
                planEntry = TaskEntry,

                NameAndDescription = Channel
            };

            Tasks.Add(t);
            UpdatePlan();
            if (t.intervalType == TaskScheduler.IntervalType.isolatedThread)
                Scheduler.CreateIsolatedThreadForPlan(t);
        }
        // bunch [connectionparams, queue]
        public void AddConnection<T>(TaskQueue.Providers.QueueConnectionParameters qcp)
            where T : TaskQueue.ITQueue
        {
            TaskQueue.ITQueue q = Activator.CreateInstance<T>();
            qcp.QueueTypeName = q.QueueType;
            qcp.QueueInstance = q;

            MessageChannels.AddConnection(qcp);
        }
        public void RegisterConnection<T>(string name, string connectionString, string database, string collection)
            where T : TaskQueue.ITQueue
        {
            AddConnection<T>(new TaskQueue.Providers.QueueConnectionParameters()
                {
                    Collection = collection,
                    Name = name,
                    Database = database,
                    ConnectionString = connectionString
                });
        }
        private void UpdatePlan()
        {
            Scheduler.SetPlan(from t in Tasks select t as TaskScheduler.PlanItem);
            //List<TaskScheduler.PlanItem> plan = new List<TaskScheduler.PlanItem>();
            //foreach (QueueTask t in Tasks)
            //{
            //    plan.Add(t.Plan);
            //}
            //Scheduler.SetPlan(plan);
        }

        private void TaskEntry(TaskScheduler.ThreadItem ti, TaskScheduler.PlanItem pi)
        {
            QueueTask task = pi as QueueTask;
            if (pi.intervalType == TaskScheduler.IntervalType.isolatedThread)
            {
                //task.Module.Producer(task.Parameters);
                ((IModIsolatedProducer)task.Module.MI).IsolatedProducer(task.Parameters);
            }
            else
            {
                switch (task.Module.Role)
                {
                    case ExecutionType.Consumer:
                        ConsumerEntry(task);
                        break;
                    case ExecutionType.Producer:
                        ProducerEntry(task);
                        break;
                }
            }
        }
        private void IsolatedTaskEntry(TaskScheduler.ThreadItem ti, TaskScheduler.PlanItem pi)
        {
            QueueTask task = pi as QueueTask;
            //task.Module.Producer(task.Parameters);
            while (!ti.StopThread)
            {
                Thread.Sleep(100);
            }
        }

        private void ConsumerEntry(QueueTask task)
        {
            Console.WriteLine("consumer: {0}", task.ChannelName);
            //QueueTask task = pi as QueueTask;
            ModMod mod = task.Module;

            // Pop item from queue
            ChannelAnteroom ch = MessageChannels.GetAnteroom(task.ChannelName);
            TaskQueue.Providers.TaskMessage item = ch.Next();

            if (item == null)
            {
                Console.WriteLine("consumer empty: {0}", task.ChannelName);
                task.Suspended = true;
                return;
            }

            if (((IModConsumer)mod.MI).Push(task.Parameters, ref item))
            {
                item.Processed = true;
                item.ProcessedTime = DateTime.UtcNow;
                ch.Update(item);
            }
        }

        public TaskMessage Pop(string channel)
        {
            ChannelAnteroom ch = MessageChannels.GetAnteroom(channel);
            TaskQueue.Providers.TaskMessage item = ch.Next();

            if (item == null)
                return null;

            item.Processed = true;
            item.ProcessedTime = DateTime.UtcNow;
            ch.Update(item);

            return item;
        }

        private void ProducerEntry(QueueTask task)
        {
            Console.WriteLine("producer: {0}", task.ChannelName);
            //QueueTask task = pi as QueueTask;
        }
        public bool PushMessage(TaskQueue.Providers.TaskMessage msg)
        {
            ChannelAnteroom ch = MessageChannels.GetAnteroomByMessage(msg.MType);
            if (ch == null)
            {
                Console.WriteLine("unknown message type: {0}", msg.MType);
                return false;
            }
            msg.AddedTime = DateTime.UtcNow;
            bool status = ch.Push(msg);

            var x = from t in Tasks
                    where t.ChannelName == ch.ChannelName
                    select t;
            foreach (QueueTask t in x)
            {
                t.Suspended = false;
            }
            return status;
        }
        public long GetChannelOccupancy(string channelName)
        {
            ChannelAnteroom ch = MessageChannels.GetAnteroom(channelName);
            return ch.CountNow;
        }
    }
}
