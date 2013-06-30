using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskBroker.Assemblys
{
    public class AssemblyModule
    {
        public string PathName { get; set; } // {...}.dll
        public bool Exists(string domainDirectory)
        {
            return File.Exists(Fullpath(domainDirectory));
        }
        public string Fullpath(string domainDirectory)
        {
            return Path.Combine(domainDirectory, PathName);
        }
    }
    public class AssemblyCard
    {
        public string PathName { get; set; }
        public Assembly assembly;
        public string[] Interfaces;
    }
    public class AssemblyHolder
    {
        public string ModulesFolder { get; set; }
        public AssemblyHolder(string folder = null)
        {
            assemblys = new List<AssemblyModule>();
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            ModulesFolder = folder == null ?
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) :
                folder;

            if (!Directory.Exists(ModulesFolder))
            {
                Directory.CreateDirectory(ModulesFolder);
            }

            loadedAssemblys = new Dictionary<string, AssemblyCard>();
            CreateDomain();
        }
        AppDomain Domain;
        public List<AssemblyModule> assemblys;
        public Dictionary<string, AssemblyCard> loadedAssemblys;
        public void ReLoadAssemblys(ModHolder moduleHolder)
        {
            loadedAssemblys.Clear();

            foreach (AssemblyModule a in assemblys)
            {
                if (!a.Exists(ModulesFolder))
                {
                    continue;
                }
                AssemblyName an = AssemblyName.GetAssemblyName(a.Fullpath(ModulesFolder));
                Assembly assembly = Domain.Load(an);

                AddModAssembly(moduleHolder, assembly, a.PathName);
            }
        }
        private void AddModAssembly(ModHolder moduleHolder, Assembly assembly, string pathname)
        {
            // get interfaces, add to dic
            AssemblyCard card = new AssemblyCard()
            {
                assembly = assembly,
                PathName = pathname
            };
            var type = typeof(IMod);
            var types = assembly.GetTypes().Where(p => type.IsAssignableFrom(p) && !p.IsInterface);
            foreach (var item in types)
            {
                moduleHolder.ModInterfaces.Add(item.FullName, item);
                moduleHolder.AddMod(item.FullName, new ModMod() { RemoteMod = true });
            }
            card.Interfaces = (from t in types
                               select t.FullName).ToArray();

            loadedAssemblys.Add(pathname, card);
            
        }
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string[] Parts = args.Name.Split(',');
            string File = Path.Combine(ModulesFolder, Parts[0].Trim() + ".dll");

            return System.Reflection.Assembly.LoadFrom(File);
        }
        public void UnloadModules()
        {
            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();

            AppDomain.Unload(Domain);

            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();

            CreateDomain();
        }
        private void CreateDomain()
        {
            Domain = AppDomain.CreateDomain("MODDOM");
        }

    }
}
