using SourceControl.BuildServers;
using SourceControl.Build;
using SourceControl.Containers;
using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TaskUniversum;
using System.Reflection;

namespace TaskBroker.Assemblys
{
    /// <summary>
    /// Runtime assembly artefacts merge and resolve <br />
    /// Prior :: dependencies collecting in order to avoid conflicts
    /// </summary>
    public class ArtefactsDepot
    {
        ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        internal class PackageAndArtefactLibLinked
        {
            public AssemblyVersionPackage versionsPackage;
            public AssemblyArtifact art;

            public System.Reflection.Assembly loadedAssembly;

            public bool IsLocalDependency { get; set; }
            public string LocalDependencyPath { get; set; }

            public BuildResultFile ExtractLibraryFile()
            {
                if (IsLocalDependency)
                {
                    return new BuildResultFile { Name = art.FileName, Data = File.ReadAllBytes(LocalDependencyPath) };
                }
                return new BuildResultFile { Name = art.FileName, Data = versionsPackage.ExtractArtefact(art) };
            }
            public BuildResultFile ExtractSymbolsFile()
            {
                if (IsLocalDependency)
                {
                    string LocalDependencyPathSym = ToSymbolsPathFromLibraryPath(LocalDependencyPath);
                    return new BuildResultFile { Name = Path.GetFileName(LocalDependencyPathSym), Data = File.ReadAllBytes(LocalDependencyPathSym) };
                }
                string prefile = ToSymbolsPathFromLibraryPath(art.FileName);
                AssemblyArtifact symart = versionsPackage.FindArtefactByName(prefile);
                if (symart == null)
                {
                    prefile = ToSymbolsPathFromLibraryPath(art.FileName, "mdb");
                    symart = versionsPackage.FindArtefactByName(prefile);
                    if (symart == null)
                        return null;
                }
                return new BuildResultFile { Name = prefile, Data = versionsPackage.ExtractArtefact(symart) };
            }
            private string ToSymbolsPathFromLibraryPath(string pathDll, string ext = "pdb")
            {
                return pathDll.Remove(pathDll.Length - 3, 3);
            }
        }
        private Dictionary<string, PackageAndArtefactLibLinked> AssetControlList;
        private Dictionary<string, PackageAndArtefactLibLinked> AssemblyControlList;
        public ArtefactsDepot()
        {
            AssetControlList = new Dictionary<string, PackageAndArtefactLibLinked>();
            AssemblyControlList = new Dictionary<string, PackageAndArtefactLibLinked>();
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                RegisterAssemblyLibrary(a);
            }
        }

        public void RegisterAssets(AssemblyVersionPackage binPackage, System.Reflection.Assembly loadedAssembly)
        {
            for (int i = 0; i < binPackage.Version.Artefacts.Count; i++)
            {
                AssemblyArtifact art = binPackage.Version.Artefacts[i];
                if (art.IsAssembly)
                {
                    PackageAndArtefactLibLinked l = new PackageAndArtefactLibLinked
                    {
                        art = art,
                        versionsPackage = binPackage,
                        loadedAssembly = loadedAssembly
                    };
                    AssetControlList.Add(binPackage.ContainerName, l);
                    if (!AssemblyControlList.ContainsKey(loadedAssembly.FullName))
                    {
                        AssemblyControlList.Add(loadedAssembly.FullName, l);
                    }
                    else
                    {
                        AssemblyControlList[loadedAssembly.FullName] = l;
                    }
                    break;
                }
            }
        }
        public void RegisterAssemblyLibrary(System.Reflection.Assembly loadedAssembly)
        {
            if (!AssemblyControlList.ContainsKey(loadedAssembly.FullName))
            {
                PackageAndArtefactLibLinked l = new PackageAndArtefactLibLinked
                {
                    IsLocalDependency = true,
                    LocalDependencyPath = loadedAssembly.Location,
                    loadedAssembly = loadedAssembly
                };

                AssemblyControlList.Add(loadedAssembly.FullName, l);
            }
        }
        public SourceControl.Build.BuildResultFile ResolveAsset(string FileName)
        {
            return null;
        }
        public bool ResolveLibraryAsset(string LibraryFullName, out BuildResultFile library, out BuildResultFile symbols)
        {
            library = symbols = null;
            PackageAndArtefactLibLinked l = null;
            if (AssemblyControlList.TryGetValue(LibraryFullName, out l))
            {
                library = l.ExtractLibraryFile();
                symbols = l.ExtractSymbolsFile();
                return true;
            }

            return false;
        }
        public bool ResolveLibraryAssembly(
            string LibraryName,
            string LibraryVersion,
            string LibraryFullName,
            string RequestingLibraryFullName,
            out System.Reflection.Assembly AssemblyLibrary)
        {
            AssemblyLibrary = null;
            PackageAndArtefactLibLinked l = null;
            if (AssemblyControlList.TryGetValue(LibraryFullName, out l))
            {
                AssemblyLibrary = l.loadedAssembly;
                return true;
            }
            if (LibraryName.StartsWith("System."))
            {
                var libScopeFallback = (from z in AssemblyControlList
                                        where z.Key.StartsWith(LibraryName + " ")
                                        select new { z.Key, z.Value }).FirstOrDefault();
                if (libScopeFallback != null)
                {
                    AssemblyLibrary = libScopeFallback.Value.loadedAssembly;
                    return true;
                }
            }
            if (AssemblyControlList.TryGetValue(RequestingLibraryFullName, out l))
            {
                if (l.IsLocalDependency)
                    return false;

                AssemblyArtifact dllart = l.versionsPackage.FindArtefactByName(LibraryName + ".dll");
                if (dllart != null)
                {
                    AssemblyArtifact dllpdbart = l.versionsPackage.FindArtefactByName(LibraryName + ".pdb");
                    if (dllpdbart == null)
                        dllpdbart = l.versionsPackage.FindArtefactByName(LibraryName + ".mdb");

                    Assembly assembly = null;
                    if (dllpdbart != null)
                        assembly = Assembly.Load(l.versionsPackage.ExtractArtefact(dllart),
                            l.versionsPackage.ExtractArtefact(dllpdbart));
                    else
                        assembly = Assembly.Load(l.versionsPackage.ExtractArtefact(dllart));

                    AssemblyControlList[assembly.FullName] = l;
                    AssemblyLibrary = assembly;
                    return true;
                }
            }

            return false;
        }
    }
}
