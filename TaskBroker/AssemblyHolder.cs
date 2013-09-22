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
        public AssemblyModule(SourceControl.Assemblys.AssemblyBinary bin)
        {
            binary = bin;
        }
        //public AssemblyModule(string dll)
        //{
        //    binary = SourceControl.Assemblys.AssemblyBinary.FromFile(dll);
        //}
        public bool SymbolsPresented
        {
            get
            {
                return binary.symbols != null;
            }
        }
        public string PathName
        {
            get
            {
                return binary.Name;
            }
        }
        public readonly SourceControl.Assemblys.AssemblyBinary binary;

        public bool RuntimeLoaded { get; private set; }
        public string RutimeLoadException { get; private set; }
    }
    public class AssemblyCard
    {
        public string AssemblyName { get; set; }
        public Assembly assembly;
        public string[] Interfaces;
    }
}
