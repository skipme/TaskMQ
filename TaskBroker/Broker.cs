﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using TaskBroker.Assemblys;
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
                10000 at 940 ms. tp 1 at ,09ms :: internal module throughput
                10000 at 3722 ms. tp 1 at ,37 :: external module throughput
         * 
         */
        public Broker(RestartApplication restartApp = null)
        {
            this.restartApp = restartApp;

            Tasks = new List<QueueTask>();
            Scheduler = new TaskScheduler.ThreadPool();
            MessageChannels = new QueueMTClassificator();

            Modules = new ModHolder(this);
        }

        //public QueueConParams Connections;
        public QueueMTClassificator MessageChannels;
        public List<QueueTask> Tasks;
        public ModHolder Modules;
        //public QueueClassificator Queues;
        public TaskScheduler.ThreadPool Scheduler;

        //modules
        //bunch[model, queue, +module] .... TODO: maybe bunch with channel better?
        //public void RegisterModule(ModMod mod)
        //{
        //    mod.InitialiseEntry(this, mod);
        //    Modules.AddModConstructed(mod);
        //}

        //public void RegisterConsumerModule<C, M>(/*string name*/)
        //    where C : IModConsumer
        //    where M : TaskMessage
        //{
        //    TaskBroker.ModMod stub = new TaskBroker.ModMod()
        //        {
        //            ModAssembly = typeof(C).Assembly,
        //            AcceptsModel = Activator.CreateInstance<M>(),
        //            Role = TaskBroker.ExecutionType.Consumer,
        //            //UniqueName = name,
        //            MI = Activator.CreateInstance<C>()
        //        };

        //    stub.InitialiseEntry(this, stub);
        //    Modules.AddModConstructed(stub);
        //}
        //public void RegisterSelfValuedModule<C>()
        //    where C : IMod
        //{
        //    var typeC = typeof(IModConsumer);

        //    TaskBroker.ModMod stub = new TaskBroker.ModMod()
        //    {
        //        ModAssembly = typeof(C).Assembly,
        //        Role = typeof(C).IsAssignableFrom(typeC) ? TaskBroker.ExecutionType.Consumer : ExecutionType.Producer,
        //        MI = Activator.CreateInstance<C>()
        //    };
        //    stub.InitialiseEntry(this, stub);
        //    Modules.AddModConstructed(stub);
        //    stub.MI.RegisterTasks(this, stub);
        //}
        //public void RegisterSelfValuedModule(Type interfaceMod, bool remote = true)
        //{
        //    var type = typeof(IMod);
        //    var typeC = typeof(IModConsumer);
        //    if (type.IsAssignableFrom(interfaceMod))
        //    {
        //        TaskBroker.ModMod stub = new TaskBroker.ModMod()
        //        {
        //            ModAssembly = interfaceMod.Assembly,
        //            Role = interfaceMod.IsAssignableFrom(typeC) ? TaskBroker.ExecutionType.Consumer : ExecutionType.Producer,
        //            MI = (IMod)Activator.CreateInstance(interfaceMod),
        //            RemoteMod = remote
        //        };
        //        stub.InitialiseEntry(this, stub);
        //        Modules.AddModConstructed(stub);
        //        stub.MI.RegisterTasks(this, stub);
        //    }
        //}
        public void RegisterSelfValuedModule(Type interfaceMod, bool remote = true)
        {
            Modules.AddMod(interfaceMod.FullName, new ModMod() { }, this);
        }

        //public void RegisterRemoteSelfValuedModule(IMod instance, string assemblyPath, bool consumer)
        //{
        //    Modules.AddRemoteMod(new ModMod()
        //    {
        //        RemoteMod = true,
        //        MI = instance,
        //        ModAssembly = assemblyPath,
        //        Role = consumer ? ExecutionType.Consumer : ExecutionType.Producer
        //    }, this);
        //}

        //public void RegisterSelfValuedModule<C>(bool remote = true)
        //    where C : IMod
        //{
        //    Modules.AddMod(typeof(C).FullName, new ModMod() { RemoteMod = remote }, this);
        //}

        ////public void RegisterMessageModel(MessageType mt)
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
            Dictionary<string, object> parameters = null, string Description = "-")
        {
            ModMod module = Modules.GetByName(moduleName);

            if (module == null)
                throw new Exception("required qmodule not found: " + moduleName);
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
                Parameters = parameters,

                intervalType = it,
                intervalValue = intervalValue,
                planEntry = ep,

                NameAndDescription = Description
            };

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
                Parameters = null,

                intervalType = mst.intervalType,
                intervalValue = mst.intervalValue,
                NameAndDescription = mst.NameAndDescription
            };
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
            //Scheduler.SetPlan(from t in Tasks select t as TaskScheduler.PlanItem);
            List<TaskScheduler.PlanItem> plan = new List<TaskScheduler.PlanItem>();
            foreach (QueueTask t in Tasks)
            {
                plan.Add(t);
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
            ChannelAnteroom ch = MessageChannels.GetAnteroom(task.ChannelName);
            TaskQueue.Providers.TaskMessage message = ch.Next();

            if (message == null)
            {
                Console.WriteLine("consumer empty: {0}", task.ChannelName);
                task.Suspended = true;
                return;
            }
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
        public void AddAssemblyByPath(string path)
        {
            Modules.AddAssembly(path);
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
            Modules.AssemblyHolder.LoadAssemblys(this);
        }
        public void RevokeBroker()
        {
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
        //public void ReloadModules()
        //{
        //    StopBroker();

        //    Modules.ReloadModules(this);
        //    for (int i = 0; i < Tasks.Count; i++)
        //    {
        //        QueueTask t = Tasks[i];
        //        ModMod module = Modules.GetByName(t.ModuleName);// TODO: extract to method - also link it to tasks registration...

        //        if (module == null)
        //            throw new Exception("required qmodule not found.");
        //        t.Module = module;
        //    }

        //    RevokeBroker();
        //}
        ~Broker()
        {
            StopBroker();
        }
    }
}
