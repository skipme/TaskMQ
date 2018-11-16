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
        public static AssemblyName GetAssemblyVersion(string assemblyPath)
        {
            AssemblyName an = AssemblyName.GetAssemblyName(assemblyPath);
            return an;
        }
        public static AssemblyName GetAssemblyVersionMono(byte[] assemblyData)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(assemblyData))
            {
                Mono.Cecil.AssemblyDefinition def = Mono.Cecil.AssemblyDefinition.ReadAssembly(ms);
                return ToAssemblyVersion(def);
            }
        }
        public static AssemblyName GetAssemblyVersionMono(string assemblyPath)
        {
            Mono.Cecil.AssemblyDefinition def = Mono.Cecil.AssemblyDefinition.ReadAssembly(assemblyPath);
            return ToAssemblyVersion(def);
        }

        private static AssemblyName ToAssemblyVersion(Mono.Cecil.AssemblyDefinition def)
        {
            Mono.Cecil.AssemblyNameReference defn = Mono.Cecil.AssemblyNameDefinition.Parse(def.FullName);

            AssemblyName n = new AssemblyName()
            {
                Name = defn.Name,
                //CultureInfo = defn.Culture,
                Version = defn.Version
            };
            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < defn.PublicKeyToken.Length; i++)
            //    sb.Append(defn.PublicKeyToken[i].ToString("x2"));

            if (defn.PublicKeyToken != null)
                n.SetPublicKeyToken(defn.PublicKeyToken);

            return n;
        }

    }
}
