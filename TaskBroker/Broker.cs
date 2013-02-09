using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace TaskBroker
{
    public class Broker
    {
        public Broker()
        {
            Tasks = new List<QueueTask>();
            Scheduler = new TaskScheduler.ThreadPool();
            Queues = new QueueClassificator();
            Modules = new ModHolder();
            MessageChannels = new QueueMTClassificator();
            Connections = new QueueConParams();

            Scheduler.Allocate();
        }
        public QueueConParams Connections;
        public QueueMTClassificator MessageChannels;
        public List<QueueTask> Tasks;
        public ModHolder Modules;
        public QueueClassificator Queues;
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
        public void RegistrateTask(string uniqueName, string modName, string NameAndDesc, TaskScheduler.IntervalType it, long intervalValue)
        {
            ModMod module = Modules.GetByName(modName);
            QueueTask t = new QueueTask()
            {
                Name = uniqueName,
                Module = module,
                Description = NameAndDesc
            };
            TaskScheduler.PlanItem p = new TaskScheduler.PlanItem()
            {
                CustomObject = t,
                NameAndDescription = NameAndDesc,
                intervalType = it,
                intervalValue = intervalValue,
                ModuleName = module.UniqueName,
                planEntry = TaskEntry
            };
            t.Plan = p;

            Tasks.Add(t);
            UpdatePlan();
            if (p.intervalType == TaskScheduler.IntervalType.isolatedThread)
                Scheduler.CreateIsolatedThreadForPlan(p);
        }
        private void UpdatePlan()
        {
            List<TaskScheduler.PlanItem> plan = new List<TaskScheduler.PlanItem>();
            foreach (QueueTask t in Tasks)
            {
                plan.Add(t.Plan);
            }
        }

        private void TaskEntry(TaskScheduler.PlanItem pi)
        {
            QueueTask task = pi.CustomObject as QueueTask;
            if (task.Plan.intervalType == TaskScheduler.IntervalType.isolatedThread)
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

        private void ConsumerEntry(TaskScheduler.PlanItem pi)
        {
            QueueTask task = pi.CustomObject as QueueTask;
            ModMod mod = task.Module;

            // Pop item from queue
            MessageType mt = MessageChannels.GetByName(task.ChannelName);
            TaskQueue.ITQueue queue = Queues.GetQueue(mt.QueueName);

            TaskQueue.Providers.QueueConnectionParameters qparams = Connections.GetByName(mt.ConnectionParameters);
            queue.InitialiseFromModel(mt.Model, qparams);
            TaskQueue.ITItem item = queue.GetItem(task.consumerSelector);

            if (item == null)
                return;

            if (mod.Consumer(task.Parameters, ref item))
            {
                item.Processed = true;
                item.ProcessedTime = DateTime.UtcNow;
                queue.UpdateItem(item);
            }
        }
        public TaskQueue.ITItem Pop(string channel)
        {
            MessageType mt = MessageChannels.GetByName(channel);
            TaskQueue.ITQueue queue = Queues.GetQueue(mt.QueueName);

            TaskQueue.Providers.QueueConnectionParameters qparams = Connections.GetByName(mt.ConnectionParameters);
            queue.InitialiseFromModel(mt.Model, qparams);
            TaskQueue.ITItem item = queue.GetItemFifo();
            
            if (item == null)
                return null;

            item.Processed = true;
            item.ProcessedTime = DateTime.UtcNow;
            queue.UpdateItem(item);

            return item;
        }
        private void ProducerEntry(TaskScheduler.PlanItem pi)
        {
            QueueTask task = pi.CustomObject as QueueTask;
        }
        public void PushMessage(TaskQueue.Providers.TaskMessage msg)
        {
            MessageType mt = MessageChannels.GetByName(msg.MType);
            if (mt == null)
            {
                Console.WriteLine("unknown message type: {0}", msg.MType);
                return;
            }

            TaskQueue.ITQueue queue = Queues.GetQueue(mt.QueueName);
            TaskQueue.Providers.QueueConnectionParameters qparams = Connections.GetByName(mt.ConnectionParameters);
            queue.InitialiseFromModel(mt.Model, qparams);

            msg.AddedTime = DateTime.UtcNow;
            queue.Push(msg);
        }
    }
}
