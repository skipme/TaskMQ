﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TaskBroker.Assemblys;
using TaskUniversum;
using TaskUniversum.Task;

namespace TaskBroker
{
    public class ModMod : IBrokerModule
    {
        public string UniqueName { get { return MI.Name; } }
        public string Description { get { return MI.Description; } }

        public string ModAssembly { get; set; }
        public ExecutionType Role { get; set; }

        public IMod MI { get; set; }
        public void ExitEntry()
        {
            MI.Exit();
        }
        public void InitialiseEntry(IBroker context, IBrokerModule thisModule)
        {
            MI.Initialise(context, thisModule);
        }
        public TaskQueue.RepresentedModel ParametersModel
        {
            get
            {
                if (Role == ExecutionType.Consumer)
                {
                    return ((IModConsumer)MI).ParametersModel != null ? 
                        new TaskQueue.RepresentedModel(((IModConsumer)MI).ParametersModel.GetType()) : 
                        TaskQueue.RepresentedModel.Empty;
                }
                return TaskQueue.RepresentedModel.Empty;
            }
        }
    }

    public class ModHolder
    {
        ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        public ModHolder(Broker b)
        {
            Modules = new Dictionary<string, ModMod>();
            ModInterfaces = new Dictionary<string, Type>();
            ModLocalInterfaces = new Dictionary<string, Type>();
            loadedInterfaces = new Dictionary<string, string>();

            AddInterfacesFromCurrentDomain(b);
        }

        public Dictionary<string, ModMod> Modules;
        public Dictionary<string, Type> ModInterfaces;
        public Dictionary<string, Type> ModLocalInterfaces;
        public Dictionary<string, string> loadedInterfaces;

        public void RegisterInterface(Type interfaceMod, string assembly)
        {
            var type = typeof(IMod);
            if (type.IsAssignableFrom(interfaceMod))
            {
                if (ModInterfaces.ContainsKey(interfaceMod.FullName))
                {
                    // error
                }
                else
                {
                    ModInterfaces.Add(interfaceMod.FullName, interfaceMod);
                    loadedInterfaces.Add(interfaceMod.FullName, assembly);
                }
            }
            else
            {
                // error
            }
        }
        public void HostModule(string interfaceName, ModMod mod, IBroker b)
        {
            Type iface = null;
            if (ModInterfaces.ContainsKey(interfaceName))
            {
                iface = ModInterfaces[interfaceName];
            }
            else if (ModLocalInterfaces.ContainsKey(interfaceName))
            {
                iface = ModLocalInterfaces[interfaceName];
            }
            else
            {
                logger.Error("the module at {0} not found", interfaceName);
                return;
                // error
            }
            var tc = typeof(IModConsumer);
            mod.Role = tc.IsAssignableFrom(iface) ? ExecutionType.Consumer : ExecutionType.Producer;
            mod.MI = (IMod)Activator.CreateInstance(iface);
            mod.ModAssembly = loadedInterfaces[interfaceName];
            if (Modules.ContainsKey(mod.UniqueName))
            {
                logger.Error("the module: {0} at {1} already registered", mod.UniqueName, interfaceName);
                return;
            }
            Modules.Add(mod.UniqueName, mod);

            // TODO: Extract to broker context
            mod.MI.Initialise(b, mod);
            MetaTask[] tasks = mod.MI.RegisterTasks(mod);
            if (tasks != null)
                foreach (MetaTask t in tasks)
                {
                    b.RegisterTempTask(t, mod);
                }
        }

        public ModMod GetInstanceByName(string name)
        {
            ModMod m;
            if (!Modules.TryGetValue(name, out m))
            {
                // logger lvl
            }
            return m;
        }

        private void AddInterfacesFromCurrentDomain(Broker b)
        {
            Type IModType = typeof(IMod);
            var types = AppDomain.CurrentDomain.GetAssemblies()//.ToList()
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(assemblyType =>
                    !assemblyType.IsInterface
                    && IModType.IsAssignableFrom(assemblyType) 
                    );

            foreach (Type item in types)
            {
                ModLocalInterfaces.Add(item.FullName, item);
                loadedInterfaces.Add(item.FullName, item.Assembly.FullName);
            }
            reloadLocalMods(b);
        }
        private void reloadLocalMods(Broker b)
        {
            foreach (Type item in ModLocalInterfaces.Values)
            {
                HostModule(item.FullName, new ModMod(), b);
            }
        }
        private void ExitMod(string name)
        {
            ModMod m = GetInstanceByName(name);
            m.ExitEntry();
        }

        ModMod this[string name]
        {
            get
            {
                return GetInstanceByName(name);
            }
        }
    }
}
