using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskBroker.Assemblys
{
    public class Assemblys
    {
        public static void ForceReferencedLoad()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

            var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();

            foreach (var path in toLoad)
            {
                try
                {
                    loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path)));
                }
                catch (Exception e)
                {
                    Console.WriteLine("assembly force load exception: {0}", e.Message);
                }
            }
        }
        public string ModulesFolder { get; set; }
        public Assemblys(string folder = null)
        {
            ModulesFolder = folder == null ?
                AppDomain.CurrentDomain.BaseDirectory :
                folder;
            list = new List<AssemblyModule>();
            loadedAssemblys = new Dictionary<string, AssemblyCard>();
            //loadedInterfaces = new Dictionary<string, string>();
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }
        public List<AssemblyModule> list;
        public Dictionary<string, AssemblyCard> loadedAssemblys;
        public Dictionary<string, string> loadedInterfaces;

        public void AddAssembly(string path)
        {
            if (loadedAssemblys.ContainsKey(path))
            {
                // error
                return;
            }
            list.Add(new AssemblyModule()
            {
                PathName = path
            });
        }
        public void LoadAssemblys(Broker b)
        {
            loadedAssemblys.Clear();
            foreach (AssemblyModule a in list)
            {
                if (!(a.IsLoaded = LoadAssembly(b, a)))
                {
                    Console.WriteLine("assembly not loaded....");// specific error channel
                }
            }
        }
        private bool LoadAssembly(Broker b, AssemblyModule a)
        {
            if (!a.Exists(ModulesFolder))
            {
                Console.WriteLine("assembly not found: '{0}'", a.Fullpath(ModulesFolder));
                return false;
            }
            try
            {
                AddAssemblyUnsafe(b, a);
            }
            catch (Exception e)
            {
                // diagnostic error channel
                Console.WriteLine("assembly loading error: '{0}' :: {1}", a.Fullpath(ModulesFolder), e.Message);
                return false;
            }
            return true;
        }

        private void AddAssemblyUnsafe(Broker b, AssemblyModule a)
        {
            Assembly assembly = Assembly.LoadFrom(a.Fullpath(ModulesFolder));
            AssemblyCard card = new AssemblyCard()
            {
                assembly = assembly,
                PathName = a.PathName
            };
            var type = typeof(IMod);
            var types = assembly.GetTypes().Where(p => type.IsAssignableFrom(p) && !p.IsInterface);
            foreach (Type item in types)
            {
                b.Modules.RegisterInterface(item, a.PathName);
                b.RegisterSelfValuedModule(item);
                //loadedInterfaces.Add(item.FullName, a.PathName);
            }
            card.Interfaces = (from t in types
                               select t.FullName).ToArray();

            loadedAssemblys.Add(a.PathName, card);
        }
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string[] Parts = args.Name.Split(',');
            string File = Path.Combine(ModulesFolder, Parts[0].Trim() + ".dll");

            return System.Reflection.Assembly.LoadFrom(File);
        }
    }
}
