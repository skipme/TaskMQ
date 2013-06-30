using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskBroker
{
    public class ModMod
    {
        public string UniqueName { get { return MI.Name; } }
        public string Description { get { return MI.Description; } }

        public TaskQueue.Providers.TItemModel AcceptsModel { get; set; }
        public TaskQueue.Providers.TItemModel ParametersModel { get; set; }

        public Assembly ModAssembly { get; set; }
        public TaskBroker.ExecutionType Role { get; set; }
        public bool RemoteMod { get; set; }

        public IMod MI { get; set; }
        public void ExitEntry()
        {
            MI.Exit();
        }
        public void InitialiseEntry(TaskBroker.Broker brokerInterface, TaskBroker.ModMod thisModule)
        {
            MI.Initialise(brokerInterface, thisModule);
        }
    }

    public class ModHolder
    {
        public ModHolder()
        {
            Modules = new Dictionary<string, ModMod>();
            ModInterfaces = new Dictionary<string, Type>();
            ModLocalInterfaces = new Dictionary<string, Type>();
            holder = new Assemblys.AssemblyHolder(Path.Combine(Directory.GetCurrentDirectory(), "assemblys"));

            AddInterfacesFromCurrentDomain();
            holder.ReLoadAssemblys(this);
        }

        public Dictionary<string, ModMod> Modules;
        public Dictionary<string, Type> ModInterfaces;
        public Dictionary<string, Type> ModLocalInterfaces;
        public Assemblys.AssemblyHolder holder;

        public void AddMod(string interfaceName, ModMod mod)
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
                // error
            }
            mod.MI = (IMod)Activator.CreateInstance(iface);
            mod.ModAssembly = iface.Assembly;

            Modules.Add(mod.UniqueName, mod);
        }
        public void AddMod(ModMod mod)
        {
            Modules.Add(mod.UniqueName, mod);
        }
        public ModMod GetByName(string name)
        {
            //ModMod m = (from mod in Modules
            //            where string.Equals(mod.UniqueName, name, StringComparison.OrdinalIgnoreCase)
            //            select mod).FirstOrDefault();
            //return m;
            ModMod m;
            if (!Modules.TryGetValue(name, out m))
            {
                // logger lvl
            }
            return m;
        }
        public void ReloadModules()
        {
            ModInterfaces.Clear();
            foreach (KeyValuePair<string, ModMod> item in Modules)
            {
                item.Value.MI.Exit();
                item.Value.MI = null;
                item.Value.ModAssembly = null;
            }
            Modules.Clear();
            holder.UnloadModules();
            holder.ReLoadAssemblys(this);
            reloadLocalMods();
        }

        private void AddInterfacesFromCurrentDomain()
        {
            var type = typeof(IMod);
            var types = AppDomain.CurrentDomain.GetAssemblies().ToList()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);
            foreach (Type item in types)
            {
                ModLocalInterfaces.Add(item.FullName, item);
            }
            reloadLocalMods();
        }
        private void reloadLocalMods()
        {
            foreach (Type item in ModLocalInterfaces.Values)
            {
                AddMod(item.FullName, new ModMod() { RemoteMod = false });
            }
        }
        private void ExitMod(string name)
        {
            ModMod m = GetByName(name);
            m.ExitEntry();
        }

        ModMod this[string name]
        {
            get
            {
                return GetByName(name);
            }
        }
    }
}
