using SourceControl.Build;
using SourceControl.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TaskUniversum;
using TaskUniversum.Assembly;

namespace TaskBroker.Assemblys
{
    public class AssemblyStatus : IAssemblyStatus
    {
        public IRevision BuildServerRev;
        public IRevision PackageRev;

        public string State { get; set; }

        public bool Loaded { get; set; }
        public string LoadedRevision { get; set; }
        public string LoadedRemarks { get; set; }

        public DateTime packagedDate { get; set; }

        public AssemblyStatus(SourceControl.Assemblys.AssemblyProject prj)
        {
            State = prj.BuildServer.GetState().ToString();

            IRevision scmBS = prj.BuildServerRevision;
            IRevision scmPck = prj.PackageRevision;

            BuildServerRev = scmBS;
            PackageRev = scmPck;

            packagedDate = prj.Versions.LastPackagedDate;
            Loaded = prj.RuntimeLoaded;
            LoadedRevision = prj.RuntimeLoadedRevision;
            LoadedRemarks = prj.RuntimeLoadedRemark;
        }
    }

    public class Assemblys
    {
        public SourceControl.Assemblys.AssemblyProjects assemblySources;
        public TaskBroker.Configuration.ExtraParameters GetBuildServersConfiguration()
        {
            TaskBroker.Configuration.ExtraParameters p = new Configuration.ExtraParameters();
            p.BuildServerTypes = new List<Configuration.ExtraParametersBS>();
            foreach (KeyValuePair<string, SourceControl.BuildServers.IBuildServer> bs in assemblySources.artifacts.BuildServers)
            {
                TaskQueue.RepresentedModel rm = bs.Value.GetParametersModel().GetModel();
                p.BuildServerTypes.Add(new TaskBroker.Configuration.ExtraParametersBS { Name = bs.Key, ParametersModel = rm.ToDeclareDictionary() });
            }
            return p;
        }

        public Assemblys()
        {
            // host packages, modules
            //list = new List<AssemblyModule>();
            loadedAssemblys = new Dictionary<string, AssemblyCard>();
            SharedManagedLibraries = new ArtefactsDepot();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            // build, update packages: 
            assemblySources = new SourceControl.Assemblys.AssemblyProjects(Directory.GetCurrentDirectory());
        }
        public IEnumerable<KeyValuePair<string, AssemblyStatus>> GetSourceStatuses()
        {
            foreach (SourceControl.Assemblys.AssemblyProject proj in assemblySources.hostedProjects)
            {
                yield return new KeyValuePair<string, AssemblyStatus>(proj.moduleName, new AssemblyStatus(proj));
            }
        }
        public void UpdatePackage(string Name)
        {
            for (int i = 0; i < assemblySources.hostedProjects.Count; i++)
            {
                if (assemblySources.hostedProjects[i].moduleName == Name)
                {
                    assemblySources.hostedProjects[i].SetUpdateDeferredFlag();
                    return;
                }
            }
        }
        public void BuildSource(string Name)
        {
            for (int i = 0; i < assemblySources.hostedProjects.Count; i++)
            {
                if (assemblySources.hostedProjects[i].moduleName == Name)
                {
                    assemblySources.hostedProjects[i].SetBuildDeferredFlag();
                    return;
                }
            }

        }
        public void FetchSource(string Name)
        {
            for (int i = 0; i < assemblySources.hostedProjects.Count; i++)
            {
                if (assemblySources.hostedProjects[i].moduleName == Name)
                {
                    assemblySources.hostedProjects[i].SetFetchDeferredFlag();
                    return;
                }
            }

        }
        //public List<AssemblyModule> list;
        public Dictionary<string, AssemblyCard> loadedAssemblys;

        private ArtefactsDepot SharedManagedLibraries;

        public void AddAssemblySource(string name, string buildServerType, Dictionary<string, object> parameters)
        {
            assemblySources.Add(name, buildServerType, parameters);
        }

        //public void AddAssembly(string name)
        //{
        //    AssemblyBinVersions ver = new AssemblyBinVersions(System.IO.Directory.GetCurrentDirectory(), name);
        //    AssemblyVersionPackage package = ver.GetLatestVersion();
        //    if (package == null)
        //    {
        //        Console.WriteLine("module not well formated, package info not present: {0}", name);
        //        return;
        //    }
        //    list.Add(new AssemblyModule(package));
        //}
        //public void AddAssembly(AssemblyVersionPackage package)
        //{
        //    list.Add(new AssemblyModule(package));
        //}
        public void LoadAssemblys(Broker b)
        {
            loadedAssemblys.Clear();
            // in order to reject only new modules -if depconflict persist-
            IEnumerable<SourceControl.Assemblys.AssemblyProject> mods = assemblySources.TakeLoadTime();
            foreach (SourceControl.Assemblys.AssemblyProject a in mods)
            {
                AssemblyVersionPackage pckg = a.Versions.GetLatestVersion();
                string remarks;
                bool loaded = a.RuntimeLoaded = LoadAssembly(b, pckg, out remarks);
                a.RuntimeLoadedRevision = pckg.Version.VersionTag;
                a.RuntimeLoadedRemark = remarks;
            }
        }
        private bool LoadAssembly(Broker b, AssemblyVersionPackage a, out string remarks)
        {
            remarks = string.Empty;
            try
            {
                SharedManagedLibraries.RegisterAssets(a);
                AddAssemblyUnsafe(b, a);
            }
            catch (Exception e)
            {
                remarks = string.Format("assembly loading error: '{0}' :: {1} :: {2}", a.ContainerName, e.Message, e.StackTrace);
                Console.WriteLine(remarks);
                return false;
            }
            return true;
        }

        private void AddAssemblyUnsafe(Broker b, AssemblyVersionPackage a)
        {
            Assembly assembly = null;
            if (a.Version.FileSymbols != null)
                assembly = Assembly.Load(a.ExtractLibrary(), a.ExtractLibrarySymbols());
            else assembly = Assembly.Load(a.ExtractLibrary());

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
            }
            card.Interfaces = (from t in types
                               select t.FullName).ToArray();

            loadedAssemblys.Add(assemblyName, card);
        }
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
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
            return null;
        }

    }
}
