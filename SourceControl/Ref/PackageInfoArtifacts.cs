using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


namespace SourceControl.Ref
{
    /// <summary>
    /// PackageInfo construct
    /// Artifact notation
    /// </summary>
    public class PackageInfoArtifact
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public bool IsAssembly { get; set; }
        public string Version { get; set; }
        public string HashCode { get; set; }

        public static PackageInfoArtifact Get(string file)
        {
            PackageInfoArtifact art = new PackageInfoArtifact();
            if (file.EndsWith(".dll"))
            {
                try
                {
                    AssemblyName an = Assemblys.AssemblyHelper.GetAssemblyVersion(file);

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
    /// PackageInfo construct
    /// Assembly executable with meta information and with artifacts and dependencies
    /// </summary>
    public class PackageInfoArtifacts
    {
        public PackageInfoArtifacts() { Artefacts = new List<PackageInfoArtifact>(); }
        public string VersionTag { get; set; }
        public string FileLibarary { get; set; }
        public string FileSymbols { get; set; }
        public string AssemblyVersion { get; set; }
        public DateTime AddedAt { get; set; }

        public List<PackageInfoArtifact> Artefacts;

        public void AddArtefact(string path, BuildServers.BuildArtifact ba)
        {
            PackageInfoArtifact dllInfo = new PackageInfoArtifact
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
            PackageInfoArtifact dllInfo = PackageInfoArtifact.Get(relatedFile);
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
