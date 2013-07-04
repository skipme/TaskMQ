using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;

namespace TaskBroker.Assemblys
{
    public class AssemblyModule
    {
        public string PathName { get; set; } // {...}.dll
        public bool IsLoaded { get; set; }
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
            //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
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
        PluginLoader pd;
        public List<AssemblyModule> assemblys;
        public Dictionary<string, AssemblyCard> loadedAssemblys;

        public void AddAssembly(string path)
        {
            assemblys.Add(new AssemblyModule()
                {
                    PathName = path
                });
        }

        public void LoadAssemblys(Broker b)
        {
            loadedAssemblys.Clear();

            foreach (AssemblyModule a in assemblys)
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
                AssemblyName an = AssemblyName.GetAssemblyName(a.Fullpath(ModulesFolder));
                //an = AssemblyName.GetAssemblyName(a.PathName);
                //Assembly assembly = Domain.Load(an);

                /*Assembly assembly =*/
                bool l = pd.GetAssembly(a.Fullpath(ModulesFolder), a.PathName);
                //AddModAssembly(b, assembly, a.PathName);
            }
            catch (Exception e)
            {
                // diagnostic error channel
                Console.WriteLine("assembly loading error: '{0}' :: {1}", a.Fullpath(ModulesFolder), e.Message);
                return false;
            }
            return true;
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return null;
            string[] Parts = args.Name.Split(',');
            string File = Path.Combine(ModulesFolder, Parts[0].Trim() + ".dll");

            //return System.Reflection.Assembly.LoadFrom(File);
            AssemblyName an = AssemblyName.GetAssemblyName(File);
            return Domain.Load(an);

            //Assembly assembly = pd.GetAssembly(File);
            //return assembly;
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
            AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
            //setup.ApplicationBase = ModulesFolder;
            //setup.ShadowCopyFiles = "true";
            Domain = AppDomain.CreateDomain("MODDOM", null, setup);

            pd = (PluginLoader)Domain.CreateInstanceAndUnwrap(
                typeof(PluginLoader).Assembly.FullName,
                typeof(PluginLoader).FullName
            );
        }
    }
    [SecurityPermission(SecurityAction.Demand, Infrastructure = true)]
    internal sealed class PluginLoader : MarshalByRefObject, IDisposable
    {
        public bool GetAssembly(string AssemblyPath, string pathname)
        {
            try
            {
                AssemblyName an = AssemblyName.GetAssemblyName(AssemblyPath);
                Assembly assembly = AppDomain.CurrentDomain.Load(an);
                AddModAssembly(assembly, pathname);
                return true;
            }
            catch (Exception ex)
            {
                //throw new InvalidOperationException(ex.Message);
            }
            return false;
        }

        public void Dispose()
        {

        }
        private void AddModAssembly(Assembly assembly, string pathname)
        {
            // get interfaces, add to dic
            //AssemblyCard card = new AssemblyCard()
            //{
            //    assembly = assembly,
            //    PathName = pathname
            //};
            var type = typeof(IMod);
            var types = assembly.GetTypes().Where(p => type.IsAssignableFrom(p) && !p.IsInterface);
            foreach (Type item in types)
            {
                //b.Modules.RegisterInterface(item);
                //b.RegisterSelfValuedModule(item);
                //moduleHolder.ModInterfaces.Add(item.FullName, item);
                //moduleHolder.AddMod(item.FullName, new ModMod() { RemoteMod = true });
            }
            //card.Interfaces = (from t in types
            //                   select t.FullName).ToArray();

            //loadedAssemblys.Add(pathname, card);
        }
    }
    class ProxyDomain : MarshalByRefObject
    {
        public Assembly GetAssembly(string AssemblyPath, string ff)
        {
            try
            {
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase = ff;
                return Assembly.LoadFrom(AssemblyPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }
    }
}
