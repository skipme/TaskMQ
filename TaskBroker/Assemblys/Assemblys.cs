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
using TaskUniversum.Common;

namespace TaskBroker.Assemblys
{
    public class AssemblyStatus : IAssemblyStatus
    {
        public IRevision BuildServerRev { get; set; }
        public IRevision PackageRev { get; set; }

        public string State { get; set; }

        public bool Loaded { get; set; }
        public string LoadedRevision { get; set; }
        public string LoadedRemarks { get; set; }

        public DateTime packagedDate { get; set; }

        public AssemblyStatus(SourceControl.BuildServers.SourceController prj)
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

    public class AssemblyPackages : ISourceManager
    {
        ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        public SourceControl.BuildServers.AssemblyProjects assemblySources;
        /// <summary>
        /// list of assemblys loaded by taskmq /debug info/
        /// </summary>
        public Dictionary<string, AssemblyCard> loadedAssemblys;
        /// <summary>
        /// list of assemblys loaded by taskmq
        /// for resolving / reloading / fetching ...
        /// </summary>
        private ArtefactsDepot SharedManagedLibraries;

        public IRepresentedConfiguration GetJsonBuildServersConfiguration()
        {
            return GetBuildServersConfiguration();
        }

        public TaskBroker.Configuration.ExtraParameters GetBuildServersConfiguration()
        {
            TaskBroker.Configuration.ExtraParameters p = new Configuration.ExtraParameters();
            p.BuildServerTypes = new List<Configuration.ExtraParametersBS>();
            foreach (KeyValuePair<string, SourceControl.BuildServers.IBuildServer> bs in assemblySources.artifacts.BuildServersRegister)
            {
                TaskQueue.RepresentedModel rm = bs.Value.GetParametersModel().GetModel();
                p.BuildServerTypes.Add(new TaskBroker.Configuration.ExtraParametersBS
                {
                    Name = bs.Value.Name,
                    Description = bs.Value.Description,
                    ParametersModel = rm.schema.ToList().ToDictionary((keyItem) => keyItem.Value1, (valueItem) => new Configuration.SchemeValueSpec(valueItem.Value2))
                    //rm.ToDeclareDictionary()
                });
            }
            return p;
        }
        public bool CheckBuildServerParameters(string BSTypeName, Dictionary<string, object> bsParameters, out string explain)
        {
            SourceControl.BuildServers.IBuildServer bs;
            if ((bs = assemblySources.artifacts.GetNewInstance(BSTypeName)) != null)
            {
                bs.SetParameters(bsParameters);

                bool result = bs.CheckParameters(out explain);
                return result;
            }
            explain = string.Empty;
            return false;
        }
        public AssemblyPackages()
        {
            // host packages, modules
            //list = new List<AssemblyModule>();
            loadedAssemblys = new Dictionary<string, AssemblyCard>();
            SharedManagedLibraries = new ArtefactsDepot();

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

            // build, update packages: 
            assemblySources = new SourceControl.BuildServers.AssemblyProjects(Directory.GetCurrentDirectory());
        }

        public IEnumerable<KeyValuePair<string, IAssemblyStatus>> GetSourceStatuses()
        {
            foreach (SourceControl.BuildServers.SourceController proj in assemblySources.hostedProjects)
            {
                yield return new KeyValuePair<string, IAssemblyStatus>(proj.PackageName, new AssemblyStatus(proj));
            }
        }
        public void DoPackageCommand(string Name, SourceControllerJobs job)
        {
            for (int i = 0; i < assemblySources.hostedProjects.Count; i++)
            {
                if (assemblySources.hostedProjects[i].PackageName == Name)
                {
                    assemblySources.hostedProjects[i].DoControl(job);
                    logger.Debug("passed command {1} to package {0}", Name, job.ToString());
                    return;
                }
            }
        }

        public List<SourceControllerJobs> GetAllowedCommands(string Name)
        {
            List<SourceControllerJobs> result = null;
            for (int i = 0; i < assemblySources.hostedProjects.Count; i++)
            {
                if (assemblySources.hostedProjects[i].PackageName == Name)
                {
                    result = assemblySources.hostedProjects[i].GetAllowedJobs();
                }
            }
            // todo: exception
            return result;
        }
        public void AddAssemblySource(string name, string buildServerType, Dictionary<string, object> parameters)
        {
            assemblySources.Add(name, buildServerType, parameters);
        }

        public void LoadAssemblys(Broker b)
        {
            loadedAssemblys.Clear();
            // load local dep assemblys

            //SortedSet<string> loadedAssemblyList = new SortedSet<string>(from lasm in AppDomain.CurrentDomain.GetAssemblies()
            //                                                             select lasm.FullName);
            //IEnumerable<AssemblyName> notLoadedReferences = from assembly in AppDomain.CurrentDomain.GetAssemblies()
            //                                                from refAssembly in assembly.GetReferencedAssemblies()
            //                                                group refAssembly by refAssembly.FullName into fullname
            //                                                where !loadedAssemblyList.Contains(fullname.Key)
            //                                                select fullname.First();
            ////SourceControl.Assemblys.AssemblyHelper.GetAssemblyReferences()
            //foreach (AssemblyName assemblyName in notLoadedReferences)
            //{
            //    AppDomain.CurrentDomain.Load(assemblyName);
            //}


            // in order to reject only new modules -if depconflict persist-
            IEnumerable<SourceControl.BuildServers.SourceController> mods = assemblySources.TakeLoadTime();
            foreach (SourceControl.BuildServers.SourceController a in mods)
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
                Assembly assembly = TryRegisterAssembly(b, a);
                SharedManagedLibraries.RegisterAssets(a, assembly);
            }
            catch (Exception e)
            {
                remarks = string.Format("assembly loading error: '{0}' :: {1} :: {2}", a.ContainerName, e.Message, e.StackTrace);
                logger.Exception(e, "load assembly", "error loading assembly package ContainerName: {0}", a.ContainerName);
                return false;
            }
            return true;
        }

        private Assembly TryRegisterAssembly(Broker b, AssemblyVersionPackage a)
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
            var IModType = typeof(IMod);
            var types = assembly.GetExportedTypes().Where(assemblyType =>
                !assemblyType.IsInterface
                && IModType.IsAssignableFrom(assemblyType));

            foreach (Type item in types)
            {
                b.Modules.RegisterInterface(item, assemblyName);
                b.RegisterSelfValuedModule(item);
            }
            card.Interfaces = (from t in types
                               select t.FullName).ToArray();

            loadedAssemblys.Add(assemblyName, card);

            return assembly;
        }
        void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            SharedManagedLibraries.RegisterAssemblyLibrary(args.LoadedAssembly);
            logger.Debug("Assembly loaded: \n {0},\n located:\n {1}", args.LoadedAssembly.FullName, args.LoadedAssembly.Location);
        }
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly lib;
            if (SharedManagedLibraries.ResolveLibraryAssembly(args.Name, out lib))
            {
                return lib;
            }
            else
            {
                // Console.WriteLine("loading shared library failed: not found {0}", Parts[0]);
                logger.Error("loading shared library failed: not found {0}", args.Name);
            }
            return null;

            string[][] Parts = (from x in args.Name.Split(',')
                                select x.Trim().Split('=')).ToArray();

            string assemblyVersion = (from x in Parts
                                      where x[0] == "Version"
                                      select x[1]).First();

            //BuildResultFile asset;
            //BuildResultFile assetsym;

            //if (SharedManagedLibraries.ResolveLibrary(Parts[0][0], out asset, out assetsym))
            //{
            //    if (assetsym != null)
            //    {
            //        return Assembly.Load(asset.Data, assetsym.Data);
            //    }
            //    else
            //    {
            //        return Assembly.Load(asset.Data);
            //    }
            //}
            //else
            //{
            //    // Console.WriteLine("loading shared library failed: not found {0}", Parts[0]);
            //    logger.Error("loading shared library failed: not found {0}", Parts[0]);
            //}
            //return null;
        }


    }
}
