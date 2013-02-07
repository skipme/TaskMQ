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
        public Dictionary<string, TaskQueue.TItemValue_Type> AcceptedParameters { get; set; }

        public Assembly ModAssembly { get; set; }
        public TaskBroker.ExecutionType InvokeAs { get; set; }
        public TaskBroker.ConsumerEntryPoint Consumer { get; set; }
        public TaskBroker.ProducerEntryPoint Producer { get; set; }
    }

    public class ModHolder
    {
        public List<ModMod> Modules { get; set; }
        public ModHolder() { Modules = new List<ModMod>(); }

        public void AddMod(Assembly assembly)
        {

        }
        public void AddMod(ModMod mod)
        {
            Modules.Add(mod);
        }
        public ModMod GetByName(string name)
        {
            ModMod m = (from mod in Modules
                        where string.Compare(mod.UniqueName, name, StringComparison.OrdinalIgnoreCase) == 0
                        select mod).FirstOrDefault();
            return m;

        }
    }
}
