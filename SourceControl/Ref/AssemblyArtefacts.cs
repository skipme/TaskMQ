using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


namespace SourceControl.Ref
{
    public class AssemblyArtifact
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public bool IsAssembly { get; set; }
        public string Version { get; set; }
        public string HashCode { get; set; }

        //public static AssemblyArtifact Get(byte[] data)
        //{
        //    AssemblyArtifact art = new AssemblyArtifact();
        //    using (MemoryStream ms = new MemoryStream(data))
        //    {
        //        try
        //        {
        //            Mono.Cecil.AssemblyDefinition def = Mono.Cecil.AssemblyDefinition.ReadAssembly(ms);
        //            Mono.Cecil.AssemblyNameReference defn = Mono.Cecil.AssemblyNameDefinition.Parse(def.FullName);

        //            art.Version = defn.Version.ToString();
        //            art.Name = defn.Name;
        //            art.IsAssembly = true;
        //        }
        //        catch
        //        {

        //        }
        //    }

        //    return art;
        //}

        public static AssemblyArtifact Get(string file)
        {
            AssemblyArtifact art = new AssemblyArtifact();
            if (file.EndsWith(".dll"))
            {
                try
                {
                    AssemblyName an = AssemblyName.GetAssemblyName(file);

                    art.Version = an.Version.ToString();
                    art.Name = an.Name;
                    art.IsAssembly = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("I: {0}", e.Message);
                }
            }
            return art;
        }
    }
    /// <summary>
    /// Assembly executable with meta information and with artifacts and dependencies
    /// </summary>
    public class AssemblyArtifacts
    {
        public AssemblyArtifacts() { Artefacts = new List<AssemblyArtifact>(); }
        public string VersionTag { get; set; }
        public string FileLibarary { get; set; }
        public string FileSymbols { get; set; }
        public string AssemblyVersion { get; set; }
        public DateTime AddedAt { get; set; }

        public List<AssemblyArtifact> Artefacts;

        public void AddArtefact(string path, BuildServers.BuildArtifact ba)
        {
            AssemblyArtifact dllInfo = new AssemblyArtifact
            {
                FileName = path,
                IsAssembly = ba.IsAssembly,
                Name = ba.Name,
                Version = ba.Version
            };
            Artefacts.Add(dllInfo);
        }
        public void AddArtefact(string name, string relatedFile)
        {
            AssemblyArtifact dllInfo = AssemblyArtifact.Get(relatedFile);
            dllInfo.FileName = name;
            Artefacts.Add(dllInfo);
        }
        //public void AddArtefact(string name, byte[] relatedFile)
        //{
        //    AssemblyArtifact dllInfo = AssemblyArtifact.Get(relatedFile);
        //    dllInfo.FileName = name;
        //    if (!dllInfo.IsAssembly)
        //        dllInfo.Name = name;
        //    Artefacts.Add(dllInfo);
        //}
    }

}
