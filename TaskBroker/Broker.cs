using System;
using System.Collections.Generic;
using System.Linq;
using TaskBroker.Configuration;
using TaskBroker.Statistics;
using TaskQueue.Providers;
using TaskScheduler;
using TaskUniversum;
using TaskUniversum.Common;
using TaskUniversum.Statistics;
using TaskUniversum.Task;

namespace TaskBroker
{
    public class Broker : IBroker
    {
        ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        public RestartApplication restartApp { get; set; }
        public RestartApplication resetBroker { get; set; }

        /*
         * consumer stub throughput ratings
                10000 at 940 ms. tp 1 at ,09ms :: internal module throughput (same appdomain)
                10000 at 3722 ms. tp 1 at ,37 :: external module throughput (cross appdomain)
         * 
         */
        public Broker(RestartApplication restartApp = null, RestartApplication resetBroker = null)
        {
            this.restartApp = restartApp;
            this.resetBroker = resetBroker;

            logger.Debug("intitialising broker");
            // logger for modules only
            ModulesLogger = new TaskBroker.Logger.CommonTape(new Logger.LoggerEndpoint[]{
                new Logger.ConsoleEndpoint()
            });

            ClearConfiguration();

            Statistics = new StatHub();
            Scheduler = new TaskScheduler.ThreadPool();
            AssemblyHolder = new Assemblys.AssemblyPackages();

            MaintenanceTasks = new List<PlanItem>()
            {
                // include statistic flush task
                new PlanItem(){
                     intervalType = IntervalType.intervalSeconds,
                     intervalValue = 15,
                     NameAndDescription="statistic maintenance task",
                     JobEntry = (ThreadContext ti, PlanItem pi)=>{ Statistics.FlushRetairedChunks(); return 1;
                     }
                },
                // 
                //new PlanItem(){
                //     intervalType = IntervalType.intervalSeconds,
                //     intervalValue = 30,
                //     NameAndDescription="assemblies maintenance task",
                //     JobEntry = (ThreadContext ti, PlanItem pi)=>
                //     { 
                //         AssemblyHolder.assemblySources.FetchAllIfRequired();
                //         AssemblyHolder.assemblySources.BuildAllIfRequired();
                //         AssemblyHolder.assemblySources.UpdateAllIfRequired();
                //     }
                //}

                // TODO:
                // performance tune
                // by channel level growing -> increment tasks for channel -> increment working threads
                // check for throughput if doesn't change -> rollback to default execution plan
                //new PlanItem(){
                //     intervalType = IntervalType.intervalSeconds,
                //     intervalValue = 10,
                //     NameAndDescription="channel throughput tune",
                //     planEntry = (ThreadItem ti, PlanItem pi)=>{  }
                //}
                // with 10sec interval->update quilifier
                // with 30sec with quilifier data do recommendations from recommedationsHub
            };

            Modules = new ModHolder(this);

            QueueInterfaces = new QueueClassificator();

            Configurations = new ConfigurationDepot();
        }
        private void ClearConfiguration()
        {
            MessageChannels = new MessageTypeClassificator();
            Tasks = new List<QueueTask>();

            if (Modules != null)
                Statistics.Clear();

            if (Modules != null)
                foreach (var mod in Modules.Modules.Values)
                {
                    MetaTask[] tasks = mod.MI.RegisterTasks(mod);
                    if (tasks != null)
                        foreach (MetaTask t in tasks)
                        {
                            this.RegisterTempTask(t, mod);
                        }
                }
        }
        private void LoadLatestConfiguration(bool tasksOnly = false)
        {
            if (!tasksOnly)// isAssemblyConfiguration reloading required
            {
                LoadAssemblyConfiguration();
            }

            ClearConfiguration();
            ConfigurationBroker cmain = Configurations.GetNewestMainConfiguration();

            if (cmain != null)
                cmain.Apply(this);// validate and apply
        }

        private void LoadAssemblyConfiguration()
        {
            ConfigurationAssemblys casm = Configurations.GetNewestAssemblysConfiguration();
            if (casm != null)
            {
                casm.Apply(this);
                LoadAssemblys();
            }
        }
        private void CreateRandomBenchConfiguration(bool tasksOnly = false)
        {
            if (!tasksOnly)// isAssemblyConfiguration reloading required
            {
                LoadAssemblyConfiguration();
            }
            ClearConfiguration();
            ConfigurationBroker cmain = Configurations.GetNewestMainConfiguration();

            cmain.Apply(this);

            var qinterface = QueueInterfaces.GetQueue(MemQueue.queueTypeName);
            QueueSpecificParameters parameters = new MemQueueParams();
            RegisterConnection("benchQ", qinterface, parameters, true);
            RegisterChannel("benchQ", "benchCH#" + 1, true);
            Random rnd = new Random(DateTime.UtcNow.Millisecond);
            for (int i = 0; i < 10000; i++)
            {
                RegisterTempTask(new MetaTask
                {
                    ChannelName = "benchCH#1",
                    intervalType = IntervalType.intervalMilliseconds,
                    intervalValue = rnd.Next(0, 1000),
                    ModuleName = BenchModules.ModConsumer.ModuleName,
                    NameAndDescription = "bench task #" + (i + 1)
                }, null);
            }
        }
        public ConfigurationDepot Configurations;
        public MessageTypeClassificator MessageChannels;
        public List<QueueTask> Tasks;
        public List<PlanItem> MaintenanceTasks;

        Logger.CommonTape ModulesLogger;
        public ModHolder Modules;
        public Assemblys.AssemblyPackages AssemblyHolder;
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
        public long GetChannelLevel(string messageType)
        {
            ChannelAnteroom ch = MessageChannels.GetAnteroomByMessage(messageType);
            return ch.Queue.GetQueueLength();
        }
        // bunch [model, queue, +channel]
        //public void RegisterChannel<T>(string connectionName, string channelName)
        //    where T : TaskQueue.Providers.TItemModel
        //{
        //    MessageChannels.AddMessageChannel<T>(new TaskBroker.MessageChannel()
        //    {
        //        ConnectionName = connectionName,
        //        UniqueName = channelName
        //    });
        //}
        // bunch [queue, +channel]
        public void RegisterChannel(string connectionName, string channelName, bool Temporary)
        {
            if (!MessageChannels.Connections.ContainsKey(connectionName))
            {
                throw new Exception("Can't find queue " + connectionName + " declare queue first");
            }
            MessageChannels.AddMessageChannel(new TaskBroker.MessageChannel()
            {
                ConnectionName = connectionName,
                UniqueName = channelName,
                //MessageType = MType
                Temporary = Temporary
            });
        }
        // bunch [channel~[message model], module[message model], +executionContext]
        // note: this is configure which channel is selected for custom module
        public void RegisterTask(string ChannelName, string moduleName,
            IntervalType it = IntervalType.intervalMilliseconds,
            long intervalValue = 100,
            Dictionary<string, object> parameters = null, string Description = "-")
        {
            ModMod module = Modules.GetInstanceByName(moduleName);
            MessageChannel channel = MessageChannels.GetInstanceByName(ChannelName);

            if (module == null)
                throw new Exception("RegisterTask: required module not found: " + moduleName);

            if (channel == null)
                throw new Exception("RegisterTask: required channel not found: " + ChannelName);

            TaskScheduler.PlanItemEntryPoint ep = TaskEntry;
            if (it == IntervalType.isolatedThread)
            {
                ep = IsolatedTaskEntry;
            }
            QueueTask t = new QueueTask()
            {
                Module = module,
                ModuleName = moduleName,

                //Description = Description,
                ChannelName = ChannelName,
                Anteroom = ChannelName == null ? null : MessageChannels.GetAnteroom(ChannelName),
                Parameters = parameters,

                intervalType = it,
                intervalValue = intervalValue,
                JobEntry = ep,

                NameAndDescription = Description
            };
            // task not required a channel only if module not implement consumer interface
            if (module.Role == ExecutionType.Consumer)
            {
                if (!typeof(IModConsumer).IsAssignableFrom(module.MI.GetType()))
                {
                    throw new Exception("Consumer module required a consumer interface");
                }
                if (ChannelName == null)
                {
                    throw new Exception("Consumer module required a channel");
                }
                else
                {
                    if (t.Anteroom.ChannelStatsIn == null && t.Anteroom.ChannelStatsIn == null)// first task for this channel?
                    {
                        // monitoring put operation
                        t.Anteroom.ChannelStatsIn = Statistics.InitialiseModel(new BrokerStat("chan_in", ChannelName));
                        t.Anteroom.ChannelStatsOut = Statistics.InitialiseModel(new BrokerStat("chan_out", ChannelName));
                        // set selector
                        TaskQueue.TQItemSelector selector = ((IModConsumer)module.MI).ConfigureSelector();
                        // channel -> model(MType)
                        MessageChannels.AssignMessageTypeToChannel(ChannelName, ((IModConsumer)module.MI).AcceptsModel, moduleName);
                        channel.consumerSelector = selector;
                    }
                }
            }
            Tasks.Add(t);
            UpdatePlan();
            //if (t.intervalType == TaskScheduler.IntervalType.isolatedThread)
            //    Scheduler.CreateIsolatedThreadForPlan(t);
        }
        public void RegisterTempTask(MetaTask mst, IBrokerModule module)
        {
            if (module == null)
            {
                module = Modules.GetInstanceByName(mst.ModuleName);
                if (module == null)
                    throw new Exception("RegisterTempTask: required module not found: " + mst.ModuleName);
            }
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
            if (t.Anteroom != null && t.Anteroom.ChannelStatsIn == null)
            {
                t.Anteroom.ChannelStatsIn = Statistics.InitialiseModel(new BrokerStat("chan_in", mst.ChannelName));
                t.Anteroom.ChannelStatsOut = Statistics.InitialiseModel(new BrokerStat("chan_out", mst.ChannelName));
                //t.Anteroom.ChannelStatistic = Statistics.InitialiseModel(new BrokerStat("channel", mst.ChannelName));
            }
            //ModMod module = Modules.GetByName(t.ModuleName);
            //if (module == null)
            //    throw new Exception("required qmodule not found.");
            TaskScheduler.PlanItemEntryPoint ep = TaskEntry;
            if (t.intervalType == IntervalType.isolatedThread)
            {
                ep = IsolatedTaskEntry;
            }
            t.JobEntry = ep;

            t.Module = module;
            t.Temporary = true;

            Tasks.Add(t);
            UpdatePlan();
        }
        // bunch [connectionparams, queue] ()
        private void AddConnection(TaskQueue.Providers.QueueConnectionParameters qcp)
        {
            //qcp.QueueTypeName = qI.QueueType;
            //qcp.QueueInstance = qI;

            MessageChannels.AddConnection(qcp);
        }
        public void RegisterConnection<T>(string name, TaskQueue.Providers.QueueSpecificParameters parameters, bool Temporary)
            where T : TaskQueue.ITQueue
        {
            TaskQueue.ITQueue qm = Activator.CreateInstance<T>();
            RegisterConnection(name, qm, parameters, Temporary);
        }
        public void RegisterConnection(string name, TaskQueue.ITQueue qI, TaskQueue.Providers.QueueSpecificParameters parameters, bool Temporary)
        {
            QueueConnectionParameters qcp = new QueueConnectionParameters(name);
            qcp.SetInstance(qI, parameters);
            qcp.Temporary = Temporary;
            AddConnection(qcp);
        }
        private void UpdatePlan()
        {
            //Scheduler.SetPlan(from t in Tasks select t as TaskScheduler.PlanItem);
            List<TaskScheduler.PlanItem> plan = new List<TaskScheduler.PlanItem>();
            foreach (QueueTask t in Tasks)
            {
                plan.Add(t);
            }
            foreach (PlanItem p in MaintenanceTasks)
            {
                plan.Add(p);
            }
            Scheduler.SetPlan(plan);
        }

        private int TaskEntry(TaskScheduler.ThreadContext ti, TaskScheduler.PlanItem pi)
        {
            // TODO: remove this method , replace that routing to task registration
            QueueTask task = pi as QueueTask;
            switch (task.Module.Role)
            {
                case ExecutionType.Consumer:
                    return ConsumerEntry(task);
                    break;
                case ExecutionType.Producer:
                    return ProducerEntry(task);
                    break;
            }
            return 1;
        }
        private int IsolatedTaskEntry(TaskScheduler.ThreadContext ti, TaskScheduler.PlanItem pi)
        {
            QueueTask task = pi as QueueTask;
            try
            {
                ((IModIsolatedProducer)task.Module.MI).IsolatedProducer(task.Parameters);
            }
            catch (Exception e)
            {
                logger.Exception(e, "isolated call procedure", "module '{0}' will be turned off", task.ModuleName);
                ti.StopThread = true;
            }
            //task.Module.Producer(task.Parameters);
            while (!ti.StopThread)
            {
                System.Threading.Thread.Sleep(100);
            }
            ((IModIsolatedProducer)task.Module.MI).IsolatedProducerStop();
            return 1;
        }

        private int ConsumerEntry(QueueTask task)
        {
            //Console.WriteLine("consumer: {0}", task.ChannelName);
            //QueueTask task = pi as QueueTask;
            IBrokerModule mod = task.Module;

            // Pop item from queue
            ChannelAnteroom ch = task.Anteroom;//MessageChannels.GetAnteroom(task.ChannelName);

            if (ch.InternalEmptyFlag)
                return 1;

            TaskQueue.Providers.TaskMessage message = ch.Next();

            if (message == null)
            {
                return 1;
            }
            //if (message == null)
            //{
            //    // this must be replaced with better way communication with message bus
            //    Console.WriteLine("Consumer empty, suspended: {0}", task.ChannelName);
            //    task.Suspended = true;
            //    return;
            //}
            TaskQueue.Providers.TaskMessage item = message;

            bool updated = ((IModConsumer)mod.MI).Push(task.Parameters, ref message);
            if (updated)
            {
                message.Processed = true;
                message.ProcessedTime = DateTime.UtcNow;
            }

            updated = updated || (!Object.ReferenceEquals(item, message));// model is modified
            if (updated)
            {
                ch.Update(message);
            }
            ch.ChannelStatsOut.inc(); // inc stats
            return 0;
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

        private int ProducerEntry(QueueTask task)
        {
            logger.Debug("(not implemented...) producer: {0}", task.ChannelName);
            return 1;
        }
        public TaskQueue.RepresentedModel GetValidationModel(string MessageType, string ChannelName = null)
        {
            // find all modules with messageType and channelname
            MessageChannel channel = MessageChannels.GetChannelForMessage(MessageType);
            if (channel == null || channel.AssignedMessageModel == null)
                return null;
            return new TaskQueue.RepresentedModel(channel.AssignedMessageModel.GetType());
        }

        public bool PushMessage(TaskQueue.Providers.TaskMessage msg)
        {
            ChannelAnteroom ch = MessageChannels.GetAnteroomByMessage(msg.MType);
            if (ch == null)
            {
                logger.Warning("push: unknown message type: {0}", msg.MType);
                return false;
            }

            //ch.ChannelStatsIn.inc();
            //return true;

            msg.AddedTime = DateTime.UtcNow;
            bool status = ch.Push(msg);

            if (status)
            {
                ch.ChannelStatsIn.inc();
            }
            else
            {
                logger.Warning("push: can't enqueue message mtype: {0}, channel: {1}", msg.MType, ch.ChannelName);
            }
            // TODO: replace with suspend interface
            //var x = from t in Tasks
            //        where t.ChannelName == ch.ChannelName
            //        select t;
            //foreach (QueueTask t in x)
            //{
            //    t.Suspended = false;
            //}
            // ~

            return status;
        }
        public long GetChannelOccupancy(string channelName)
        {
            ChannelAnteroom ch = MessageChannels.GetAnteroom(channelName);
            return ch.CountNow;
        }
        public void AddAssembly(string name, string buildServer, Dictionary<string, object> parameters)
        {
            AssemblyHolder.AddAssemblySource(name, buildServer, parameters);
        }
        /// <summary>
        /// deprecated
        /// </summary>
        /// <param name="name"></param>
        //public void AddAssemblyByName(string name)
        //{
        //    //Modules.AddAssembly(path);
        //    AssemblyHolder.AddAssembly(name);
        //}
        public void ReloadAssemblys()
        {
            // just restart application
            // delay restart...
            if (restartApp != null)
                restartApp();
            else
                logger.Error("Can't restart application");
        }
        //
        public void StopBroker()
        {
            // stop scheduler
            // 
            Scheduler.SuspendAll();
            while (Scheduler.Activity)
            {
                System.Threading.Thread.Sleep(100);
            }
            //stop isolated threads...
            Scheduler.CloseIsolatedThreads();
            //Tasks.RemoveAll(x => x.Temp);
            logger.Info("Broker has been stopped...");
        }
        public void LoadAssemblys()
        {
            AssemblyHolder.LoadAssemblys(this);
        }
        public void RevokeBroker(bool BenchConfiguration, bool reconfigureFromStorage, bool reconfigureOnlyTasks)
        {
            if (reconfigureFromStorage)
            {
                if (BenchConfiguration)
                    CreateRandomBenchConfiguration(reconfigureOnlyTasks);
                else
                    LoadLatestConfiguration(reconfigureOnlyTasks);
            }
            Scheduler.ReWake();
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


        public IEnumerable<KeyValuePair<string, TaskUniversum.Assembly.IAssemblyStatus>> GetSourceStatuses()
        {
            return AssemblyHolder.GetSourceStatuses();
        }

        public ISourceManager GetSourceManager()
        {
            return AssemblyHolder;
        }

        public StatisticContainer GetChannelsStatistic()
        {
            StatisticContainer statc = new StatisticContainer();
            foreach (BrokerStat sm in MessageChannels.GetStatistics())
            {
                if (sm == null)
                    continue;
                StatRange range = sm.GetFlushedMinRange();

                statc.FlushedMinRanges.Add(new MetaStatRange() { Name = sm.Name, range = range });
            }
            return statc;
        }

        public void RegisterNewConfiguration(string id, string body)
        {
            Configurations.RegisterConfiguration(id, body);
        }


        public bool ValidateAndCommitConfigurations(string MainID, string ModulesID, string AssembliesID, out string errors, bool Reset = false, bool Restart = false)
        {
            bool result = true;
            errors = string.Empty;

            if (!string.IsNullOrWhiteSpace(MainID))
                result = Configurations.ValidateAndCommitMain(MainID, out errors);

            if (result && !string.IsNullOrWhiteSpace(ModulesID))
                result = Configurations.ValidateAndCommitMods(ModulesID, out errors);

            if (result && !string.IsNullOrWhiteSpace(AssembliesID))
                result = Configurations.ValidateAndCommitAssemblies(AssembliesID, out errors);

            if (result)
                Configurations.ClearConfigurationsForCommit();// drop all pending commits

            if (result)
            {
                if (Reset)
                {
                    // delay reset....
                    resetBroker();
                }
                else if (Restart)
                {
                    // delay restart....
                    restartApp();
                }
            }

            return result;
        }
        public string GetCurrentConfigurationString(bool Main, bool Modules, bool Assemblys, bool Extra)
        {
            //if (Main)
            //    return TaskBroker.Configuration.BrokerConfiguration.ExtractFromBroker(this).SerialiseJsonString();
            //else if (Modules)
            //    return TaskBroker.Configuration.BrokerConfiguration.ExtractModulesFromBroker(this).SerialiseJsonString();
            //else if (Assemblys)
            //    return TaskBroker.Configuration.BrokerConfiguration.ExtractAssemblysFromBroker(this).SerialiseJsonString();
            //else if (Extra)
            //{
            //    return GetSourceManager().GetJsonBuildServersConfiguration();
            //}

            //return string.Empty;
            return GetCurrentConfiguration(Main, Modules, Assemblys, Extra).SerialiseJsonString();
        }
        public IRepresentedConfiguration GetCurrentConfiguration(bool Main, bool Modules, bool Assemblys, bool Extra)
        {
            if (Main)
                return TaskBroker.Configuration.BrokerConfiguration.ExtractFromBroker(this);
            else if (Modules)
                return TaskBroker.Configuration.BrokerConfiguration.ExtractModulesFromBroker(this);
            else if (Assemblys)
                return TaskBroker.Configuration.BrokerConfiguration.ExtractAssemblysFromBroker(this);
            else if (Extra)
            {
                return GetSourceManager().GetJsonBuildServersConfiguration();
            }

            return new RepresentedConfiguration();
        }
        public ILogger APILogger()
        {
            return TaskUniversum.ModApi.ScopeLogger.GetClassLogger(ModulesLogger, 2);
        }

        /// <summary>
        /// Message type assigned to channel with task registration, one channel can operate one mtype
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetCurrentChannelMTypeMap()
        {
            return MessageChannels.MChannelsList.ToDictionary(o => o.UniqueName, o => o.AssignedMessageType);
        }

        public void DoPackageCommand(string Name, SourceControllerJobs job)
        {
            Scheduler.DeferJob((tc, j) => { AssemblyHolder.DoPackageCommand(Name, job); return 1; });
        }
    }
}
