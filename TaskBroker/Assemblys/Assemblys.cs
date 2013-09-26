using SourceControl.Build;
using SourceControl.Containers;
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
        //public static void ForceReferencedLoad()// because JIT
        //{
        //    var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        //    var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

        //    var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
        //    var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();

        //    foreach (var path in toLoad)
        //    {
        //        try
        //        {
        //            loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path)));
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine("assembly force load exception: {0}", e.Message);
        //        }
        //    }
        //}
        public string ModulesFolder { get; private set; }
        public Assemblys()
        {
            list = new List<AssemblyModule>();
            loadedAssemblys = new Dictionary<string, AssemblyCard>();
            //loadedInterfaces = new Dictionary<string, string>();
            SharedManagedLibraries = new ArtefactsDepot();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public List<AssemblyModule> list;
        public Dictionary<string, AssemblyCard> loadedAssemblys;
        //private AssemblyModule CurrentLoadingAssemblyModule;
        private ArtefactsDepot SharedManagedLibraries;

        public void AddAssembly(string name)
        {
            AssemblyBinVersions ver = new AssemblyBinVersions(System.IO.Directory.GetCurrentDirectory(), name);
            AssemblyVersionPackage package = ver.GetLatestVersion();
            if (package == null)
            {
                Console.WriteLine("module not well formated, package info xml not present: {0}", name);
                return;
            }
            list.Add(new AssemblyModule(package));
        }
        public void AddAssembly(AssemblyVersionPackage package)
        {
            list.Add(new AssemblyModule(package));
        }
        public void LoadAssemblys(Broker b)
        {
            loadedAssemblys.Clear();
            //loadedInterfaces.Clear();
            // in order to reject only new modules -if depconflict persist-
            foreach (AssemblyModule a in list.OrderBy(am => am.package.Version.AddedAt))
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
                //CurrentLoadingAssemblyModule = a;
                SharedManagedLibraries.RegisterAssets(a.package);
                AddAssemblyUnsafe(b, a);
            }
            catch (Exception e)
            {
                // diagnostic error channel
                Console.WriteLine("assembly loading error: '{0}' :: {1}", a.PathName, e.Message);
                return false;
            }
            //CurrentLoadingAssemblyModule = null;
            return true;
        }

        private void AddAssemblyUnsafe(Broker b, AssemblyModule a)
        {
            Assembly assembly = null;
            if (a.SymbolsPresented)
                assembly = Assembly.Load(a.package.ExtractLibrary(), a.package.ExtractLibrarySymbols());
            else assembly = Assembly.Load(a.package.ExtractLibrary());

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
            //if (CurrentLoadingAssemblyModule != null)
            //{
            //    string[] Parts = args.Name.Split(',');
            //    string File = Parts[0].Trim() + ".dll";
            //    string FileSym = Parts[0].Trim() + ".pdb";
            //    SourceControl.Assemblys.AssemblyAsset asset;
            //    SourceControl.Assemblys.AssemblyAsset assetsym;
            //    if (CurrentLoadingAssemblyModule.binary.assets.TryGetValue(File.ToLower(), out asset))
            //    {
            //        Console.WriteLine("loading artefact {1} in {0} for {2}", CurrentLoadingAssemblyModule.PathName, asset.Name, source);
            //        if (CurrentLoadingAssemblyModule.binary.assets.TryGetValue(FileSym.ToLower(), out assetsym))
            //        {
            //            return Assembly.Load(asset.Data, assetsym.Data);
            //        }
            //        else
            //        {
            //            return Assembly.Load(asset.Data);
            //        }
            //    }
            //}

            string[] Parts = args.Name.Split(',');
            BuildResultFile asset;
            BuildResultFile assetsym;
            if (SharedManagedLibraries.ResolveLibrary(Parts[0], out asset, out assetsym))
            {
                if (assetsym != null)
                {
                    return Assembly.Load(asset.Data, assetsym.Data);
                }
                else
                {
                    return Assembly.Load(asset.Data);
                }
            }
            else
            {
                Console.WriteLine("loading shared library failed: not found {0}", Parts[0]);
            }

            // TODO: use artefacts depot
            return null;
        }

    }
}
