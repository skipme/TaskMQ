using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using TaskBroker.Assemblys;
using TaskBroker.Configuration;
using TaskBroker.Statistics;
using TaskQueue.Providers;
using TaskScheduler;

namespace TaskBroker
{
    public class Broker
    {
        public delegate void RestartApplication();
        RestartApplication restartApp;
        /*
         * consumer stub throughput ratings
                10000 at 940 ms. tp 1 at ,09ms :: internal module throughput (same appdomain)
                10000 at 3722 ms. tp 1 at ,37 :: external module throughput (cross appdomain)
         * 
         */
        public Broker(RestartApplication restartApp = null)
        {
            this.restartApp = restartApp;
            Statistics = new StatHub();

            Tasks = new List<QueueTask>();
            Scheduler = new TaskScheduler.ThreadPool();
            OtherTasks = new List<PlanItem>()
            {
                // include statistic flush task
                new PlanItem(){
                     intervalType = IntervalType.intervalSeconds,
                     intervalValue = StatRange.seconds30,
                     NameAndDescription="statistic maintenance task",
                     planEntry = (ThreadItem ti, PlanItem pi)=>{ Statistics.FlushRetairedChunks(); }
                }
            };

            MessageChannels = new QueueMTClassificator();

            Modules = new ModHolder(this);
            AssemblyHolder = new Assemblys.Assemblys();
            QueueInterfaces = new QueueClassificator();

            Configurations = new ConfigurationDepot();
        }

        private void LoadLatestConfiguration()
        {
            ConfigurationAssemblys casm = Configurations.GetNewestAssemblysConfiguration();
            if (casm != null)
            {
                casm.Apply(this);
                LoadAssemblys();
            }
            ConfigurationBroker cmain = Configurations.GetNewestMainConfiguration();
            
            if (cmain != null)
                cmain.Apply(this);
        }

        public ConfigurationDepot Configurations;
        public QueueMTClassificator MessageChannels;
        public List<QueueTask> Tasks;
        public List<PlanItem> OtherTasks;

        public ModHolder Modules;
        public Assemblys.Assemblys AssemblyHolder;
        public QueueClassificator QueueInterfaces;
        public TaskScheduler.ThreadPool Scheduler;
        public StatHub Statistics;

        public ConfigurationBroker c_apmain;

        //modules
        //bunch[model, queue, +module] .... TODO: maybe bunch with channel better?

        public void RegisterSelfValuedModule(Type interfaceMod, bool remote = true)
        {
            Modules.HostModule(interfaceMod.FullName, new ModMod() { }, this);
        }

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
        public void RegisterChannel(string connectionName, string channelName, string MType)
        {
            MessageChannels.AddMessageChannel(new TaskBroker.MessageChannel()
            {
                ConnectionName = connectionName,
                UniqueName = channelName,
                MessageType = MType
            });
        }
        // bunch [channel, module, +executionContext]
        // note: this is configure which channel is selected for custom module
        public void RegisterTask(string Channel, string moduleName,
            IntervalType it = IntervalType.intervalMilliseconds,
            long intervalValue = 100,
            Dictionary<string, object> parameters = null, string Description = "-")
        {
            ModMod module = Modules.GetByName(moduleName);

            if (module == null)
                throw new Exception("task+: required qmodule not found: " + moduleName);
            TaskScheduler.PlanItemEntryPoint ep = TaskEntry;
            if (it == TaskScheduler.IntervalType.isolatedThread)
            {
                ep = IsolatedTaskEntry;
            }
            QueueTask t = new QueueTask()
            {
                Module = module,
                ModuleName = moduleName,

                //Description = Description,
                ChannelName = Channel,
                Anteroom = Channel == null ? null : MessageChannels.GetAnteroom(Channel),
                Parameters = parameters,

                intervalType = it,
                intervalValue = intervalValue,
                planEntry = ep,

                NameAndDescription = Description
            };
            if (t.Anteroom != null)
            {
                t.Anteroom.ChannelStatistic = Statistics.FindModel(new BrokerStat("channel", Channel));
            }
            Tasks.Add(t);
            UpdatePlan();
            //if (t.intervalType == TaskScheduler.IntervalType.isolatedThread)
            //    Scheduler.CreateIsolatedThreadForPlan(t);
        }
        public void RegisterTempTask(ModuleSelfTask mst, ModMod module)
        {
            QueueTask t = new QueueTask()
            {
                ModuleName = mst.ModuleName,

                //Description = mst.NameAndDescription,
                ChannelName = mst.ChannelName,
                Anteroom = mst.ChannelName == null ? null : MessageChannels.GetAnteroom(mst.ChannelName),
                Parameters = null,

                intervalType = mst.intervalType,
                intervalValue = mst.intervalValue,
                NameAndDescription = mst.NameAndDescription
            };
            if (t.Anteroom != null)
            {
                t.Anteroom.ChannelStatistic = Statistics.FindModel(new BrokerStat("channel", mst.ChannelName));
            }
            //ModMod module = Modules.GetByName(t.ModuleName);
            //if (module == null)
            //    throw new Exception("required qmodule not found.");
            TaskScheduler.PlanItemEntryPoint ep = TaskEntry;
            if (t.intervalType == TaskScheduler.IntervalType.isolatedThread)
            {
                ep = IsolatedTaskEntry;
            }
            t.planEntry = ep;
            t.Module = module;
            t.Temp = true;

            Tasks.Add(t);
            UpdatePlan();
        }
        // bunch [connectionparams, queue]
        private void AddConnection(TaskQueue.Providers.QueueConnectionParameters qcp, TaskQueue.ITQueue qI)
        //where T : TaskQueue.ITQueue
        {
            //TaskQueue.ITQueue q = (TaskQueue.ITQueue)Activator.CreateInstance(qType);
            qcp.QueueTypeName = qI.QueueType;
            qcp.QueueInstance = qI;

            MessageChannels.AddConnection(qcp);
        }
        public void RegisterConnection<T>(string name, string connectionString, string database, string collection)
            where T : TaskQueue.ITQueue
        {
            TaskQueue.ITQueue q = Activator.CreateInstance<T>();
            AddConnection(new TaskQueue.Providers.QueueConnectionParameters()
                {
                    Collection = collection,
                    Name = name,
                    Database = database,
                    ConnectionString = connectionString
                }, q);
        }
        public void RegisterConnection(string name, string connectionString, string database, string collection, TaskQueue.ITQueue qI)
        {
            AddConnection(new TaskQueue.Providers.QueueConnectionParameters()
            {
                Collection = collection,
                Name = name,
                Database = database,
                ConnectionString = connectionString
            }, qI);
        }
        private void UpdatePlan()
        {
            //Scheduler.SetPlan(from t in Tasks select t as TaskScheduler.PlanItem);
            List<TaskScheduler.PlanItem> plan = new List<TaskScheduler.PlanItem>();
            foreach (QueueTask t in Tasks)
            {
                plan.Add(t);
            }
            foreach (PlanItem p in OtherTasks)
            {
                plan.Add(p);
            }
            Scheduler.SetPlan(plan);
        }

        private void TaskEntry(TaskScheduler.ThreadItem ti, TaskScheduler.PlanItem pi)
        {
            QueueTask task = pi as QueueTask;
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
        private void IsolatedTaskEntry(TaskScheduler.ThreadItem ti, TaskScheduler.PlanItem pi)
        {
            QueueTask task = pi as QueueTask;
            ((IModIsolatedProducer)task.Module.MI).IsolatedProducer(task.Parameters);
            //task.Module.Producer(task.Parameters);
            while (!ti.StopThread)
            {
                Thread.Sleep(100);
            }
            ((IModIsolatedProducer)task.Module.MI).IsolatedProducerStop();
        }

        private void ConsumerEntry(QueueTask task)
        {
            //Console.WriteLine("consumer: {0}", task.ChannelName);
            //QueueTask task = pi as QueueTask;
            ModMod mod = task.Module;

            // Pop item from queue
            ChannelAnteroom ch = task.Anteroom;//MessageChannels.GetAnteroom(task.ChannelName);


            TaskQueue.Providers.TaskMessage message = ch.Next();

            if (message == null)
            {
                //Console.WriteLine("consumer empty: {0}", task.ChannelName);
                task.Suspended = true;
                return;
            }
            ch.ChannelStatistic.inc();
            //System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();
            //for (int i = 0; i < 10000; i++)
            //{
            //    ((IModConsumer)mod.MI).Push(task.Parameters, ref item);
            //}
            //w.Stop();
            //Console.WriteLine("10000 at {0} ms. tp 1 at {1:.00}", w.ElapsedMilliseconds, w.ElapsedMilliseconds / 10000.0);
            TaskQueue.Providers.TaskMessage item = message;

            bool updated = ((IModConsumer)mod.MI).Push(task.Parameters, ref message);
            if (updated)
            {
                message.Processed = true;
                message.ProcessedTime = DateTime.UtcNow;
            }
            updated = updated || (!Object.ReferenceEquals(item, message));
            if (updated)
                ch.Update(message);
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
        }
        public bool PushMessage(TaskQueue.Providers.TaskMessage msg)
        {
            ChannelAnteroom ch = MessageChannels.GetAnteroomByMessage(msg.MType);
            if (ch == null)
            {
                Console.WriteLine("push: unknown message type: {0}", msg.MType);
                return false;
            }
            msg.AddedTime = DateTime.UtcNow;
            bool status = ch.Push(msg);

            // TODO: replace with suspend interface
            var x = from t in Tasks
                    where t.ChannelName == ch.ChannelName
                    select t;
            foreach (QueueTask t in x)
            {
                t.Suspended = false;
            }
            // ~

            return status;
        }
        public long GetChannelOccupancy(string channelName)
        {
            ChannelAnteroom ch = MessageChannels.GetAnteroom(channelName);
            return ch.CountNow;
        }
        public void AddAssemblyByName(string name)
        {
            //Modules.AddAssembly(path);
            AssemblyHolder.AddAssembly(name);
        }
        public void ReloadAssemblys()
        {
            // just restart application
            if (restartApp != null)
                restartApp();
            else
                Console.WriteLine("Can't restart application");
        }
        //
        public void StopBroker()
        {
            // stop scheduler
            // 
            Scheduler.SuspendAll();
            while (Scheduler.Activity)
            {
                Thread.Sleep(100);
            }
            //stop isolated threads...
            Scheduler.CloseIsolatedThreads();
            //Tasks.RemoveAll(x => x.Temp);
            Console.WriteLine("Broker has been stopped...");
        }
        public void LoadAssemblys()
        {
            AssemblyHolder.LoadAssemblys(this);
            //Modules.AssemblyHolder.LoadAssemblys(this);
        }
        public void RevokeBroker(bool reconfigureFromStorage = false)
        {
            if (reconfigureFromStorage)
                LoadLatestConfiguration();
            Scheduler.Revoke();
            // start isolated tasks:
            foreach (var tiso in (from t in Tasks
                                  where t.intervalType == IntervalType.isolatedThread
                                  select t))
            {
                Scheduler.CreateIsolatedThreadForPlan(tiso);
            }

            UpdatePlan();
        }

        ~Broker()
        {
            StopBroker();
        }
    }
}
