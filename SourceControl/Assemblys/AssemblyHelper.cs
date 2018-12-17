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
            // TODO: add pk and culture
            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < defn.PublicKeyToken.Length; i++)
            //    sb.Append(defn.PublicKeyToken[i].ToString("x2"));

            if (defn.PublicKeyToken != null)
                n.SetPublicKeyToken(defn.PublicKeyToken);

            return n;
        }

        public class ReferencedAssembly
        {
            public string parentAssemblyFullName;
            public AssemblyName SelfName;
            public override string ToString()
            {
                return string.Format("{0}\n {1}", parentAssemblyFullName, SelfName.FullName);
            }
        }
        //public static List<ReferencedAssembly> GetAllReferencedAssemblys(Assembly src_assembly)
        //{
        //    List<ReferencedAssembly> result = new List<ReferencedAssembly>();
        //    List<string> queued = new List<string>();
        //    Queue<Assembly> assemblys = new Queue<Assembly>();
            
        //    queued.Add(src_assembly.FullName);
        //    assemblys.Enqueue(src_assembly);

        //    while (assemblys.Count > 0)
        //    {
        //        Assembly c_asm = assemblys.Dequeue();

        //        AssemblyName[] refs_ = c_asm.GetReferencedAssemblies();
        //        for (int i = 0; i < refs_.Length; i++)
        //        {           
        //            if (!queued.Contains(refs_[i].FullName))
        //            {
        //                Assembly asm_loaded_ = Assembly.Load(refs_[i]);
        //                assemblys.Enqueue(asm_loaded_);
        //                queued.Add(refs_[i].FullName);
        //            }

        //            result.Add(new ReferencedAssembly { parentAssemblyFullName = c_asm.FullName, SelfName = refs_[i] });
        //        }
        //    }
        //    return result;
        //}
    }
}
