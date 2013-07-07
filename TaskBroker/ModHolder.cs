using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TaskBroker.Assemblys;

namespace TaskBroker
{
    [Serializable]
    public class ModMod
    {
        public string UniqueName { get { return MI.Name; } }
        public string Description { get { return MI.Description; } }

        public TaskQueue.Providers.TItemModel AcceptsModel { get; set; }
        public TaskQueue.Providers.TItemModel ParametersModel { get; set; }

        public string ModAssembly { get; set; }
        public TaskBroker.ExecutionType Role { get; set; }

        public IMod MI { get; set; }
        public void ExitEntry()
        {
            MI.Exit();
        }
        public void InitialiseEntry(TaskBroker.ModMod thisModule)
        {
            MI.Initialise(thisModule);
        }
    }

    public class ModHolder
    {
        public ModHolder(Broker b)
        {
            Modules = new Dictionary<string, ModMod>();
            ModInterfaces = new Dictionary<string, Type>();
            ModLocalInterfaces = new Dictionary<string, Type>();
            loadedInterfaces = new Dictionary<string, string>();

            //AssemblyHolder = new Assemblys.AssemblyHolder(Path.Combine(Directory.GetCurrentDirectory(), "assemblys"));
            AssemblyHolder = new Assemblys.Assemblys(
                Path.Combine(Directory.GetCurrentDirectory(), "assemblys")
                );

            AddInterfacesFromCurrentDomain(b);
        }

        public Dictionary<string, ModMod> Modules;
        public Dictionary<string, Type> ModInterfaces;
        public Dictionary<string, Type> ModLocalInterfaces;
        public Dictionary<string, string> loadedInterfaces;
        //public Assemblys.AssemblyHolder AssemblyHolder;
        public Assemblys.Assemblys AssemblyHolder;

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
        public void AddMod(string interfaceName, ModMod mod, Broker b)
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
                Console.WriteLine("the module at {0} not found", interfaceName);
                return;
                // error
            }
            mod.MI = (IMod)Activator.CreateInstance(iface);
            mod.ModAssembly = loadedInterfaces[interfaceName];
            if (Modules.ContainsKey(mod.UniqueName))
            {
                Console.WriteLine("the module: {0} at {1} already registered", mod.UniqueName, interfaceName);
                return;
            }
            Modules.Add(mod.UniqueName, mod);

            mod.MI.Initialise(mod);
            ModuleSelfTask[] tasks = mod.MI.RegisterTasks(mod);
            if (tasks != null)
                foreach (ModuleSelfTask t in tasks)
                {
                    b.RegisterTempTask(t, mod);
                }
        }
        //public void AddRemoteMod(ModMod mod, Broker b)
        //{
        //    //mod.ModAssembly = mod.MI.GetType().Assembly;
        //    Modules.Add(mod.UniqueName, mod);

        //    mod.MI.Initialise(mod);
        //    ModuleSelfTask[] tasks = mod.MI.RegisterTasks(mod);
        //    if (tasks != null)
        //        foreach (ModuleSelfTask t in tasks)
        //        {
        //            b.RegisterTempTask(t);
        //        }
        //}
        public void AddAssembly(string path)
        {
            AssemblyHolder.AddAssembly(path);
        }
        //public void AddModConstructed(ModMod mod)
        //{
        //    Type i = mod.MI.GetType();
        //    ModInterfaces.Add(i.FullName, i);
        //    Modules.Add(mod.UniqueName, mod);
        //}
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
        //public void ReloadModules(Broker b)
        //{
        //    //ModInterfaces.Clear();
        //    foreach (KeyValuePair<string, ModMod> item in Modules)
        //    {
        //        item.Value.MI.Exit();
        //        item.Value.MI = null;
        //        item.Value.ModAssembly = null;
        //    }
        //    Modules.Clear();
        //    //AssemblyHolder.UnloadModules();
        //    //AssemblyHolder.LoadAssemblys(b);
        //    reloadLocalMods(b);
        //}

        private void AddInterfacesFromCurrentDomain(Broker b)
        {
            var type = typeof(IMod);
            var types = AppDomain.CurrentDomain.GetAssemblies().ToList()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);
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
                AddMod(item.FullName, new ModMod(), b);
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
