using SourceControl.Assemblys;
using SourceControl.Build;
using SourceControl.Containers;
using SourceControl.Ref;
using System;
using System.Collections.Generic;
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
            public PackageVersionArtefact art;
            public BuildResultFile ExtractLibraryFile()
            {
                return new BuildResultFile { Name = art.File, Data = pckg.ExtractArtefact(art) };
            }
            public BuildResultFile ExtractSymbolsFile()
            {
                string prefile = art.File.Remove(art.File.Length - 3, 3) + "pdb";// dll -> pdb (relative path saved!)
                PackageVersionArtefact symart = pckg.FindArtefactByName(prefile);
                if (symart == null)
                    return null;
                return new BuildResultFile { Name = art.File, Data = pckg.ExtractArtefact(symart) };
            }
        }
        private Dictionary<string, PackageAndArtefactLibLinked> AssemblyControlList;
        public ArtefactsDepot()
        {
            AssemblyControlList = new Dictionary<string, PackageAndArtefactLibLinked>();
            // TODO: loading common dependecies from calling assembly
        }
        public void RegisterAssets(AssemblyVersionPackage binPackage)
        {
            for (int i = 0; i < binPackage.Version.Artefacts.Count; i++)
            {
                PackageVersionArtefact art = binPackage.Version.Artefacts[i];
                if (art.IsAssembly)
                {
                    if (AssemblyControlList.ContainsKey(art.Name))
                    {
                        if (AssemblyControlList[art.Name].art.Version != art.Version)
                            throw new Exception();
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
