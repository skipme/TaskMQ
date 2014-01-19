using SourceControl.Build;
using SourceControl.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace TaskBroker.Assemblys
{
    /// <summary>
    /// Encapsulate path specific assembly (runtime ready scm revision (prior) or dll in file system)
    /// </summary>
    public class AssemblyModule
    {
        public AssemblyModule(AssemblyVersionPackage package)
        {
            this.package = package;
        }
        public bool SymbolsPresented
        {
            get
            {
                return package.Version.FileSymbols != null;
            }
        }
        public string PathName
        {
            get
            {
                return package.Version.FileLibarary;
            }
        }
        public readonly AssemblyVersionPackage package;

        public bool RuntimeLoaded { get; set; }
        public string RutimeLoadException { get; set; }
    }
    public class AssemblyCard
    {
        public string AssemblyName { get; set; }
        public Assembly assembly;
        public string[] Interfaces;
    }
}
