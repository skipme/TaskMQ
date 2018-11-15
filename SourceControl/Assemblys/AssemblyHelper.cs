using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SourceControl.Assemblys
{
    public class AssemblyHelper
    {
        //public static string[] GetAssemblyReferences(string assemblyPath)
        //{
        //    //ModuleDefinition assembly = ModuleDefinition.ReadModule(assemblyPath);
        //    //return (from ar in assembly.AssemblyReferences
        //    //        select ar.FullName).ToArray();
        //}
        public static AssemblyName GetAssemblyVersion(string assemblyPath)
        {
            Assembly assembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
            return assembly.GetName();
        }
    }
}
