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

        public TaskQueue.QueueItemModel AcceptedModel { get; set; }
        public TaskQueue.QueueItemModel AcceptedParameters { get; set; }

        public Assembly ModAssembly { get; set; }
        public TaskBroker.ExecutionType InvokeAs { get; set; }

        public TaskBroker.ConsumerEntryPoint Consumer { get; set; }
        public TaskBroker.ProducerEntryPoint Producer { get; set; }
        public ModInitEntryPoint InitialiseEntry { get; set; }
        public StubEntryPoint ExitEntry { get; set; }
    }

    public class ModHolder
    {
        public List<ModMod> Modules { get; set; }
        public ModHolder() { Modules = new List<ModMod>(); }

        public void AddMod(Assembly assembly, Broker b = null, bool initialise = false)
        {

        }
        public void AddMod(ModMod mod)
        {
            Modules.Add(mod);
        }
        public ModMod GetByName(string name)
        {
            ModMod m = (from mod in Modules
                        where string.Equals(mod.UniqueName, name, StringComparison.OrdinalIgnoreCase)
                        select mod).FirstOrDefault();
            return m;
        }
        public void InitialiseMod(string name, Broker b)
        {
            ModMod m = GetByName(name);
            m.InitialiseEntry(b, m);
        }
        public void ExitMod(string name)
        {
            ModMod m = GetByName(name);
            m.ExitEntry();
        }
    }
}
