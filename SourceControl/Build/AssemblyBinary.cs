using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SourceControl.Build
{
    /// <summary>
    /// Runtime ready assembly with symbols, dependencies, other assets like resource files
    /// </summary>
    public class AssemblyBinaryBuildResult
    {
        public static AssemblyBinaryBuildResult FromFile(string locationLib, string locationSymbols = null, string[] assets = null)
        {
            if (!System.IO.File.Exists(locationLib))
                return null;

            AssemblyBinaryBuildResult bin = new AssemblyBinaryBuildResult();
            try
            {
                bin.LibraryPath = locationLib;
                bin.library = System.IO.File.ReadAllBytes(locationLib);
                bin.Name = Path.GetFileName(locationLib);
                if (locationSymbols != null)
                {
                    if (!System.IO.File.Exists(locationSymbols))
                        Console.WriteLine("symbols file not found: {0}", locationSymbols);
                    else
                    {
                        bin.symbols = System.IO.File.ReadAllBytes(locationSymbols);
                        bin.SymbolsPath = locationSymbols;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("unhandled exception while loading assembly: {0}: {1}", locationLib, e.Message);
                return null;
            }
            return bin;
        }
        public string LibraryPath { get; set; }
        public string SymbolsPath { get; set; }

        public byte[] library;
        public byte[] symbols;
        public string Name { get; set; }

        public string assetsRoot { get; set; }
        public string[] assets;
    }
    public class BuildResultFile
    {
        public string Name;
        public byte[] Data;
    }
}
