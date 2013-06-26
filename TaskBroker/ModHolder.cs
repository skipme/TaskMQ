using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskBroker
{
    public class ModMod
    {
        public string UniqueName { get; set; }
        public string Description { get; set; }

        public TaskQueue.QueueItemModel AcceptsModel { get; set; }
        public TaskQueue.QueueItemModel ParametersModel { get; set; }

        public Assembly ModAssembly { get; set; }
        public TaskBroker.ExecutionType Role { get; set; }

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
        //public List<ModMod> Modules { get; set; }
        public Dictionary<string, ModMod> Modules;
        public ModHolder() { Modules = new Dictionary<string, ModMod>(); }

        public void AddMod(Assembly assembly, bool initialise = false)
        {

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

        public void ExitMod(string name)
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
