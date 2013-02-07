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
        }
        public List<QueueConnectionParameters> Connections;
        public List<TaskQueue.QueueItemModel> MessageModels;
        public List<MessageType> MessageSchemas;
        public List<QueueTask> Tasks;

        public QueueClassificator Queues { get; set; }
        public TaskScheduler.ThreadPool Scheduler { get; set; }

        public void RegistrateProducer(System.Reflection.Assembly module, string UniqueName,
            TaskScheduler.IntervalType it,
            long intervalValue, ProducerEntryPoint entry)
        {
            QueueTask t = new QueueTask()
            {
                Name = UniqueName,
                ExecAs = ExecutionType.Producer,
                Producer = entry
            };
            TaskScheduler.PlanItem p = new TaskScheduler.PlanItem()
            {
                CustomObject = t,
                NameAndDescription = UniqueName,
                intervalType = it,
                intervalValue = intervalValue,
                ModuleName = module.FullName,
                planEntry = (TaskScheduler.PlanItem pi) =>
                    {
                        QueueTask task = pi.CustomObject as QueueTask;
                        task.Producer(null);
                    }
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
    }
}
