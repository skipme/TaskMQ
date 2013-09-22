using SourceControl.Assemblys;
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
        public void RegisterAssets(AssemblyBinary assets)
        {

        }
        public AssemblyAsset ResolveAsset(string name)
        {
            return null;
        }
        public bool Library(string name, out AssemblyAsset asset, out AssemblyAsset assetSym)
        {
            asset = assetSym = null;
            return false;
        }
    }
}
