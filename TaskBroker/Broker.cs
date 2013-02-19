using System;
using System.Collections.Generic;
using System.Linq;
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
            Scheduler = new TaskScheduler.ThreadPool();
            //Queues = new QueueClassificator();
            Modules = new ModHolder();
            MessageChannels = new QueueMTClassificator();
            //Connections = new QueueConParams();

            Scheduler.Allocate();
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
            Modules.InitialiseMod(mod.UniqueName, this);
        }
        public void RegistrateModule(System.Reflection.Assembly mod)
        {
            Modules.AddMod(mod);
        }

        public void RegistarateChannel(MessageType mt)
        {
            MessageChannels.Add(mt);
        }
        public void RegistrateTask(string uniqueName, string Channel, string modName, string NameAndDesc, TaskScheduler.IntervalType it, long intervalValue)
        {
            ModMod module = Modules.GetByName(modName);
            QueueTask t = new QueueTask()
            {
                Name = uniqueName,
                Module = module,
                Description = NameAndDesc,
                ChannelName = Channel
            };
            TaskScheduler.PlanItem p = new TaskScheduler.PlanItem()
            {
                CustomObject = t,
                NameAndDescription = NameAndDesc,
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
                task.Module.Producer(task.Parameters);
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
            task.Module.Producer(task.Parameters);
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
            TaskQueue.ITItem item = ch.Next();

            if (item == null)
                return;

            if (mod.Consumer(task.Parameters, ref item))
            {
                item.Processed = true;
                item.ProcessedTime = DateTime.UtcNow;
                ch.Update(item);
            }
        }

        public TaskQueue.ITItem Pop(string channel)
        {
            ChannelAnteroom ch = MessageChannels.GetByName(channel);
            TaskQueue.ITItem item = ch.Next();

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
        public void PushMessage(TaskQueue.Providers.TaskMessage msg)
        {
            ChannelAnteroom ch = MessageChannels.GetByName(msg.MType);
            if (ch == null)
            {
                Console.WriteLine("unknown message type: {0}", msg.MType);
                return;
            }
            msg.AddedTime = DateTime.UtcNow;
            ch.Push(msg);
        }
    }
}
