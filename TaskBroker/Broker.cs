using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using TaskQueue.Providers;

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

        public void RegistrateModule(ModMod mod)
        {
            Modules.AddMod(mod);
            mod.InitialiseEntry(this, mod);
        }
        //public void RegistrateCosumerModule<T>(ConsumerEntryPoint receiver, string name) where T : TaskQueue.Providers.TaskMessage
        //{
        //    //TaskBroker.ModMod stub = new TaskBroker.ModMod()
        //    //{
        //    //    //InitialiseEntry = null,
        //    //    //ExitEntry = null,
        //    //    ModAssembly = typeof(T).Assembly,
        //    //    Consumer = receiver,
        //    //    AcceptsModel = new TaskQueue.QueueItemModel(typeof(T)),
        //    //    InvokeAs = TaskBroker.ExecutionType.Consumer,
        //    //    UniqueName = name,

        //    //};
        //    //Modules.AddMod(stub);
        //}
        public void RegistrateCosumerModule<C, M>(string name)
            where C : IModConsumer
            where M : TaskMessage
        {
            TaskBroker.ModMod stub = new TaskBroker.ModMod()
                {
                    //InitialiseEntry = null,
                    //ExitEntry = null,
                    ModAssembly = typeof(C).Assembly,
                    AcceptsModel = new TaskQueue.QueueItemModel(typeof(M)),
                    InvokeAs = TaskBroker.ExecutionType.Consumer,
                    UniqueName = name,
                };
            Modules.AddMod(stub);
            stub.InitialiseEntry(this, stub);
        }
        public void RegistrateModule(System.Reflection.Assembly mod)
        {
            Modules.AddMod(mod);
        }
        public void RegistarateMessageModel(MessageType mt)
        {
            MessageChannels.AddMessageType(mt);
        }
        public void RegistarateChannel(MessageChannel mc, string messageModelName)
        {
            MessageChannels.AddMessageChannel(mc, messageModelName);
        }
        public void RegistrateTask(string uniqueName, string Channel, string modName, string Description, TaskScheduler.IntervalType it, long intervalValue, TItemModel parameters = null)
        {
            ModMod module = Modules.GetByName(modName);
            QueueTask t = new QueueTask()
            {
                Name = uniqueName,
                Module = module,
                Description = Description,
                ChannelName = Channel,
                Parameters = parameters
            };
            TaskScheduler.PlanItem p = new TaskScheduler.PlanItem()
            {
                CustomObject = t,
                NameAndDescription = uniqueName,
                intervalType = it,
                intervalValue = intervalValue,
                planEntry = TaskEntry
            };
            t.Plan = p;

            Tasks.Add(t);
            UpdatePlan();
            if (p.intervalType == TaskScheduler.IntervalType.isolatedThread)
                Scheduler.CreateIsolatedThreadForPlan(p);
        }
        public void AddConnection(TaskQueue.Providers.QueueConnectionParameters qcp)
        {
            MessageChannels.Connections.Add(qcp);
        }
        private void UpdatePlan()
        {
            List<TaskScheduler.PlanItem> plan = new List<TaskScheduler.PlanItem>();
            foreach (QueueTask t in Tasks)
            {
                plan.Add(t.Plan);
            }
            Scheduler.SetPlan(plan);
        }

        private void TaskEntry(TaskScheduler.ThreadItem ti, TaskScheduler.PlanItem pi)
        {
            QueueTask task = pi.CustomObject as QueueTask;
            if (pi.intervalType == TaskScheduler.IntervalType.isolatedThread)
            {
                //task.Module.Producer(task.Parameters);
            }
            else
            {
                switch (task.Module.InvokeAs)
                {
                    case ExecutionType.Consumer:
                        ConsumerEntry(pi);
                        break;
                    case ExecutionType.Producer:
                        ProducerEntry(pi);
                        break;
                }
            }
        }
        private void IsolatedTaskEntry(TaskScheduler.ThreadItem ti, TaskScheduler.PlanItem pi)
        {
            QueueTask task = pi.CustomObject as QueueTask;
            //task.Module.Producer(task.Parameters);
            while (!ti.StopThread)
            {
                Thread.Sleep(100);
            }
        }

        private void ConsumerEntry(TaskScheduler.PlanItem pi)
        {
            QueueTask task = pi.CustomObject as QueueTask;
            ModMod mod = task.Module;

            // Pop item from queue
            ChannelAnteroom ch = MessageChannels.GetByName(task.ChannelName);
            TaskQueue.Providers.TaskMessage item = ch.Next();

            if (item == null)
                return;

            //if (mod.Consumer(task.Parameters, ref item))
            //{
            //    item.Processed = true;
            //    item.ProcessedTime = DateTime.UtcNow;
            //    ch.Update(item);
            //}
        }

        public TaskMessage Pop(string channel)
        {
            ChannelAnteroom ch = MessageChannels.GetByName(channel);
            TaskQueue.Providers.TaskMessage item = ch.Next();

            if (item == null)
                return null;

            item.Processed = true;
            item.ProcessedTime = DateTime.UtcNow;
            ch.Update(item);

            return item;
        }

        private void ProducerEntry(TaskScheduler.PlanItem pi)
        {
            QueueTask task = pi.CustomObject as QueueTask;
        }
        public bool PushMessage(TaskQueue.Providers.TaskMessage msg)
        {
            ChannelAnteroom ch = MessageChannels.GetByName(msg.MType);
            if (ch == null)
            {
                Console.WriteLine("unknown message type: {0}", msg.MType);
                return false;
            }
            msg.AddedTime = DateTime.UtcNow;
            return ch.Push(msg);
        }
        public long GetChannelOccupancy(string channelName)
        {
            ChannelAnteroom ch = MessageChannels.GetByName(channelName);
            return ch.CountNow;
        }
    }
}
