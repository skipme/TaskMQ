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
        public string UniqueName { get; set; }
        public string Description { get; set; }

        public TaskQueue.Providers.TItemModel AcceptsModel { get; set; }
        public TaskQueue.Providers.TItemModel ParametersModel { get; set; }

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
        public ModHolder()
        {

            CreateDomain();
            Modules = new Dictionary<string, ModMod>();
            ModInterfaces = new Dictionary<string, IMod>();
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //try
            //{
            //    Assembly assembly = System.Reflection.Assembly.Load(args.Name);
            //    if (assembly != null)
            //        return assembly;
            //}
            //catch
            //{ // ignore load error 
            //}

            // *** Try to load by filename - split out the filename of the full assembly name
            // *** and append the base path of the original assembly (ie. look in the same dir)
            // *** NOTE: this doesn't account for special search paths but then that never
            //           worked before either.
            string[] Parts = args.Name.Split(',');
            string File = @"\bin\Debug" + "\\" + Parts[0].Trim() + ".dll";

            return System.Reflection.Assembly.LoadFrom(File);
            //AssemblyName an = AssemblyName.GetAssemblyName(File);
            //Assembly assembly = Holder.Load(an);
            //return assembly;
            //return null;
        }

        AppDomain Holder;
        public Dictionary<string, ModMod> Modules;
        public Dictionary<string, IMod> ModInterfaces;

        public void AddMod(string assemblyDefferedPath)
        {
            AssemblyName an = AssemblyName.GetAssemblyName(assemblyDefferedPath);
            Assembly assembly = Holder.Load(an);

            //foreach (AssemblyName item in assembly.GetReferencedAssemblies())
            //{
            //    AssemblyName a = AssemblyName.GetAssemblyName(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assemblyDefferedPath), item.FullName+ ".dll"));
            //    Holder.Load(a);
            //}

            AddModAssembly(assembly);
        }

        private void AddModAssembly(Assembly assembly)
        {
            // get interfaces, add to dic
            var type = typeof(IMod);
            //var types = AppDomain.CurrentDomain.GetAssemblies().ToList()
            //    .SelectMany(s => s.GetTypes())
            //    .Where(p => type.IsAssignableFrom(p));
            var types = assembly.GetTypes().Where(p => type.IsAssignableFrom(p));

        }
        public void AddMod(string interfaceName, ModMod mod)
        {
            Type iface = ModInterfaces[interfaceName].GetType();
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
        public void UnloadModules()
        {
            ModInterfaces.Clear();
            foreach (KeyValuePair<string, ModMod> item in Modules)
            {
                item.Value.MI.Exit();
                item.Value.MI = null;
                item.Value.ModAssembly = null;
            }
            Modules.Clear();

            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();

            AppDomain.Unload(Holder);

            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();

            CreateDomain();
        }

        private void CreateDomain()
        {
            Holder = AppDomain.CreateDomain("MODDOM");
        }

        private void AddInterfacesFromCurrentDomain()
        {

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
