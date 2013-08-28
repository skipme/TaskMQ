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
        public static void ForceReferencedLoad()// because JIT, should drop after assembly aggregation implemented
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
        public string ModulesFolder { get; private set; }
        public Assemblys()
        {
            list = new List<AssemblyModule>();
            loadedAssemblys = new Dictionary<string, AssemblyCard>();
            //loadedInterfaces = new Dictionary<string, string>();
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }
        public List<AssemblyModule> list;
        public Dictionary<string, AssemblyCard> loadedAssemblys;
        private AssemblyModule CurrentLoadingAssemblyModule;

        public void AddAssembly(string name)
        {
            SourceControl.Assemblys.AssemblyBinVersions ver = new SourceControl.Assemblys.AssemblyBinVersions(
                System.IO.Directory.GetCurrentDirectory(), name);
            SourceControl.Assemblys.AssemblyBinary bin;
            string rev;
            ver.GetLatestVersion(out rev, out bin);
            list.Add(new AssemblyModule(bin));
        }
        public void AddAssembly(SourceControl.Assemblys.AssemblyBinary binary)
        {
            list.Add(new AssemblyModule(binary));
        }
        public void LoadAssemblys(Broker b)
        {
            loadedAssemblys.Clear();
            //loadedInterfaces.Clear();
            foreach (AssemblyModule a in list)
            {
                LoadAssembly(b, a);
                //if (!(a.IsLoaded = LoadAssembly(b, a)))
                //{
                //    Console.WriteLine("assembly not loaded....");// specific error channel
                //}
            }
        }
        private bool LoadAssembly(Broker b, AssemblyModule a)
        {
            try
            {
                CurrentLoadingAssemblyModule = a;
                AddAssemblyUnsafe(b, a);
            }
            catch (Exception e)
            {
                // diagnostic error channel
                Console.WriteLine("assembly loading error: '{0}' :: {1}", a.PathName, e.Message);
                return false;
            }
            CurrentLoadingAssemblyModule = null;
            return true;
        }

        private void AddAssemblyUnsafe(Broker b, AssemblyModule a)
        {
            Assembly assembly = null;
            if (a.SymbolsPresented)
                assembly = Assembly.Load(a.binary.library, a.binary.symbols);
            else assembly = Assembly.Load(a.binary.library);

            string assemblyName = assembly.GetName().Name;
            AssemblyCard card = new AssemblyCard()
            {
                assembly = assembly,
                AssemblyName = assemblyName
            };
            var type = typeof(IMod);
            var types = assembly.GetTypes().Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

            foreach (Type item in types)
            {
                b.Modules.RegisterInterface(item, assemblyName);
                b.RegisterSelfValuedModule(item);
                //loadedInterfaces.Add(item.FullName, a.PathName);
            }
            card.Interfaces = (from t in types
                               select t.FullName).ToArray();

            loadedAssemblys.Add(assemblyName, card);
        }
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //string source = args.RequestingAssembly.FullName.Split(',')[0].Trim().ToLower();
            //for (int i = 0; i < list.Count; i++)
            //{
            //    if (source == list[i].PathName.ToLower())
            //    {
            //        string[] Parts = args.Name.Split(',');
            //        string File = Parts[0].Trim() + ".dll";
            //        string FileSym = Parts[0].Trim() + ".pdb";
            //        SourceControl.Assemblys.AssemblyAsset asset;
            //        SourceControl.Assemblys.AssemblyAsset assetsym;
            //        if (list[i].binary.assets.TryGetValue(File.ToLower(), out asset))
            //        {
            //            if (list[i].binary.assets.TryGetValue(FileSym.ToLower(), out assetsym))
            //            {
            //                return Assembly.Load(asset.Data, assetsym.Data);
            //            }
            //            else
            //            {
            //                return Assembly.Load(asset.Data);
            //            }
            //        }
            //    }
            //}
            if (CurrentLoadingAssemblyModule != null)
            {
                string[] Parts = args.Name.Split(',');
                string File = Parts[0].Trim() + ".dll";
                string FileSym = Parts[0].Trim() + ".pdb";
                SourceControl.Assemblys.AssemblyAsset asset;
                SourceControl.Assemblys.AssemblyAsset assetsym;
                if (CurrentLoadingAssemblyModule.binary.assets.TryGetValue(File.ToLower(), out asset))
                {
                    if (CurrentLoadingAssemblyModule.binary.assets.TryGetValue(FileSym.ToLower(), out assetsym))
                    {
                        return Assembly.Load(asset.Data, assetsym.Data);
                    }
                    else
                    {
                        return Assembly.Load(asset.Data);
                    }
                }
            }
            return null;
        }
    }
}
