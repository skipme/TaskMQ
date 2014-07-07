using SourceControl.Assemblys;
using SourceControl.Build;
using SourceControl.Containers;
using SourceControl.Ref;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TaskBroker.Assemblys
{
    /// <summary>
    /// Runtime assembly artefacts merge and resolve <br />
    /// Prior :: dependencies collecting in order to avoid conflicts
    /// </summary>
    public class ArtefactsDepot
    {
        internal class PackageAndArtefactLibLinked
        {
            public AssemblyVersionPackage pckg;
            public AssemblyArtifact art;

            public bool IsLocalDependency { get; set; }
            public string LocalDependencyPath { get; set; }

            public BuildResultFile ExtractLibraryFile()
            {
                if (IsLocalDependency)
                {
                    return new BuildResultFile { Name = art.FileName, Data = File.ReadAllBytes(LocalDependencyPath) };
                }
                return new BuildResultFile { Name = art.FileName, Data = pckg.ExtractArtefact(art) };
            }
            public BuildResultFile ExtractSymbolsFile()
            {
                if (IsLocalDependency)
                {
                    string LocalDependencyPathSym = ToSymbolsPathFromLibraryPath(LocalDependencyPath);
                    return new BuildResultFile { Name = Path.GetFileName(LocalDependencyPathSym), Data = File.ReadAllBytes(LocalDependencyPathSym) };
                }
                string prefile = ToSymbolsPathFromLibraryPath(art.FileName);
                AssemblyArtifact symart = pckg.FindArtefactByName(prefile);
                if (symart == null)
                {
                    prefile = ToSymbolsPathFromLibraryPath(art.FileName, "mdb");
                    symart = pckg.FindArtefactByName(prefile);
                    if (symart == null)
                        return null;
                }
                return new BuildResultFile { Name = prefile, Data = pckg.ExtractArtefact(symart) };
            }
            private string ToSymbolsPathFromLibraryPath(string pathDll, string ext = "pdb")
            {
                return pathDll.Remove(pathDll.Length - 3, 3);
            }
        }
        private Dictionary<string, PackageAndArtefactLibLinked> AssemblyControlList;
        public ArtefactsDepot()
        {
            AssemblyControlList = new Dictionary<string, PackageAndArtefactLibLinked>();
            // TODO: loading common dependecies from calling assembly
            RegisterWorkingDirAssemblys();
        }
        private void RegisterWorkingDirAssemblys()
        {
            string[] files = Directory.GetFiles(Environment.CurrentDirectory);
            foreach (string fpath in files)
            {
                AssemblyArtifact art = AssemblyArtifact.Get(fpath);
                if (art.IsAssembly)
                {
                    if (AssemblyControlList.ContainsKey(art.Name))
                    {
                        if (AssemblyControlList[art.Name].art.Version != art.Version)// this can't be happen
                            throw new Exception();
                    }
                    else
                    {
                        PackageAndArtefactLibLinked l = new PackageAndArtefactLibLinked
                        {
                            IsLocalDependency = true,
                            art = art,
                            LocalDependencyPath = fpath
                        };

                        AssemblyControlList.Add(art.Name, l);
                    }
                }
            }
        }

        /// <summary>
        /// This check for conflicts between all modules references <br/>
        ///     this thing about that you don't have isolation to make sure all modules are up to date
        /// </summary>
        /// <param name="binPackage"></param>
        public void RegisterAssets(AssemblyVersionPackage binPackage)
        {
            for (int i = 0; i < binPackage.Version.Artefacts.Count; i++)
            {
                AssemblyArtifact art = binPackage.Version.Artefacts[i];
                if (art.IsAssembly)
                {
                    if (AssemblyControlList.ContainsKey(art.Name))
                    {
                        if (AssemblyControlList[art.Name].art.Version != art.Version)
                            throw new Exception(string.Format("The module {0} has incosistent dependency '{1}'({2}) with other modules - '{3}'({4}): {5}",
                                binPackage.ContainerName, art.Name, art.Version,
                                AssemblyControlList[art.Name].art.Name, AssemblyControlList[art.Name].art.Version,
                                AssemblyControlList[art.Name].IsLocalDependency ? "WorkingDir Dependency" : "Module dependency"));
                    }
                    else
                    {
                        PackageAndArtefactLibLinked l = new PackageAndArtefactLibLinked
                        {
                            art = art,
                            pckg = binPackage
                        };

                        AssemblyControlList.Add(art.Name, l);
                    }
                }
            }
        }
        public SourceControl.Build.BuildResultFile ResolveAsset(string FileName)
        {
            return null;
        }
        public bool ResolveLibrary(string LibraryName, out BuildResultFile library, out BuildResultFile symbols)
        {
            library = symbols = null;
            PackageAndArtefactLibLinked l = null;
            if (AssemblyControlList.TryGetValue(LibraryName, out l))
            {
                library = l.ExtractLibraryFile();
                symbols = l.ExtractSymbolsFile();
                return true;
            }

            return false;
        }
    }
}
