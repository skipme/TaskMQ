using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SourceControl.Assemblys
{
    /// <summary>
    /// Runtime ready assembly with symbols, dependencies, other assets like resource files
    /// </summary>
    public class AssemblyBinary
    {
        public AssemblyBinary()
        {
            assets = new Dictionary<string, AssemblyAsset>();
        }
        public static AssemblyBinary FromFile(string locationLib, string locationSymbols = null, string[] assets = null)
        {
            if (!System.IO.File.Exists(locationLib))
                return null;

            AssemblyBinary bin = new AssemblyBinary();
            try
            {
                bin.library = System.IO.File.ReadAllBytes(locationLib);
                bin.Name = Path.GetFileName(locationLib);
                if (locationSymbols != null)
                {
                    if (!System.IO.File.Exists(locationSymbols))
                        Console.WriteLine("symbols file not found: {0}", locationSymbols);
                    else
                    {
                        bin.symbols = System.IO.File.ReadAllBytes(locationSymbols);
                    }
                }
                if (assets != null)
                    foreach (var assetloc in assets)
                    {
                        if (!System.IO.File.Exists(assetloc))
                        {
                            Console.WriteLine("asset missed");
                            continue;
                        }
                        AssemblyAsset at = new AssemblyAsset
                        {
                            Data = File.ReadAllBytes(assetloc),
                            Name = Path.GetFileNameWithoutExtension(assetloc)
                        };
                        bin.AddAsset(at);
                    }

            }
            catch (Exception e)
            {
                Console.WriteLine("unhandled exception while loading assembly: {0}: {1}", locationLib, e.Message);
                return null;
            }
            return bin;
        }

        public byte[] library;
        public byte[] symbols;
        public Dictionary<string, AssemblyAsset> assets;
        public void AddAsset(AssemblyAsset asset)
        {
            assets.Add(asset.Name.ToLower(), asset);
        }
        public string Name { get; set; }
    }
    /// <summary>
    /// Could be dll or sym or 
    /// </summary>
    public class AssemblyAsset
    {
        /// <summary>
        /// Relative / only filename
        /// </summary>
        public string Name;
        public byte[] Data;
        /// <summary>
        /// For reference
        /// </summary>
        public string Version;
        /// <summary>
        /// Recognise for the version
        /// </summary>
        public string HashCode;
    }
}
