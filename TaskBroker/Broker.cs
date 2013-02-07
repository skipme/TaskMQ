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
        }
        public List<QueueConnectionParameters> Connections;
        public List<TaskQueue.QueueItemModel> MessageModels;
        public List<MessageType> MessageSchemas;
        public List<QueueTask> Tasks;

        public ModHolder Modules { get; set; }
        public QueueClassificator Queues { get; set; }
        public TaskScheduler.ThreadPool Scheduler { get; set; }

        public void RegistrateModule(ModMod mod)
        {
            Modules.AddMod(mod);
        }
        public void RegistrateTask(string modName, string NameAndDesc, TaskScheduler.IntervalType it, long intervalValue)
        {
            ModMod module = Modules.GetByName(modName);
            QueueTask t = new QueueTask()
            {
                Name = module.UniqueName,
                Module = module
            };
            TaskScheduler.PlanItem p = new TaskScheduler.PlanItem()
            {
                CustomObject = t,
                NameAndDescription = NameAndDesc,
                intervalType = it,
                intervalValue = intervalValue,
                ModuleName = module.UniqueName,
                planEntry = (TaskScheduler.PlanItem pi) =>
                    {
                        QueueTask task = pi.CustomObject as QueueTask;
                        if (task.Plan.intervalType == TaskScheduler.IntervalType.isolatedThread)
                        {
                            task.Module.Producer(task.Parameters);
                        }
                        
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
