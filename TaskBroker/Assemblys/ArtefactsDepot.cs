using SourceControl.Assemblys;
using SourceControl.Build;
using SourceControl.Containers;
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
        public void RegisterAssets(AssemblyVersionPackage assets)
        {

        }
        public SourceControl.Build.BuildResultFile ResolveAsset(string name)
        {
            return null;
        }
        public bool ResolveLibrary(string name, out BuildResultFile asset, out BuildResultFile assetSym)
        {
            asset = assetSym = null;
            return false;
        }
    }
}
