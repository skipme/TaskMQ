using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TaskUniversum;

namespace SourceControl.BuildServers
{
    public class BuildArtifact
    {
        public byte[] Data;

        public string Name { get; set; }
        public string FileName { get; set; }
        public bool IsAssembly { get; set; }
        public string Version { get; set; }
        public string HashCode { get; set; }

        public static BuildArtifact Get(byte[] data)
        {
            BuildArtifact art = new BuildArtifact();
            using (MemoryStream ms = new MemoryStream(data))
            {
                try
                {
                    Mono.Cecil.AssemblyDefinition def = Mono.Cecil.AssemblyDefinition.ReadAssembly(ms);
                    Mono.Cecil.AssemblyNameReference defn = Mono.Cecil.AssemblyNameDefinition.Parse(def.FullName);

                    art.Version = defn.Version.ToString();
                    art.Name = defn.Name;
                    art.IsAssembly = true;
                }
                catch
                {

                }
            }

            art.Data = data;

            return art;
        }
    }
    /// <summary>
    /// built assemblies with meta information and with artifacts and dependencies
    /// </summary>
    public class BuildArtifacts
    {
        static ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        public BuildArtifacts() { Artefacts = new List<BuildArtifact>(); }

        public BuildArtifact GetArtifact(string name)
        {
            if (name == null)
                return null;

            for (int i = 0; i < Artefacts.Count; i++)
            {
                if (Artefacts[i].FileName == name)
                    return Artefacts[i];
            }
            return null;
        }
        public BuildArtifact GetArtifactByName(string name)
        {
            if (name == null)
                return null;

            for (int i = 0; i < Artefacts.Count; i++)
            {
                if (Artefacts[i].Name == name)
                    return Artefacts[i];
            }
            return null;
        }
        public string VersionTag { get; set; }
        public string AssemblyArtefactName { get; set; }
        public string AssemblyArtefactNameSym { get; set; }
        public DateTime AddedAt { get; set; }

        public List<BuildArtifact> Artefacts;

        public void AddArtefact(string name, byte[] relatedFile)
        {
            BuildArtifact dllInfo = BuildArtifact.Get(relatedFile);
            dllInfo.FileName = name;
            if (!dllInfo.IsAssembly)
                dllInfo.Name = name;
            Artefacts.Add(dllInfo);
        }

        public static BuildArtifacts FromZipArchive(string AssemblyArtefactName, string versionTag, byte[] data)
        {
            string AssemblyArtefactNameSym = pathWithoutExtension(AssemblyArtefactName) + ".pdb";

            BuildArtifacts result = new BuildArtifacts();
            FileContentArchive.ZipStream zipArch = new FileContentArchive.ZipStream(data);
            FileContentArchive.FileStorageEntry[] entrys = zipArch.GetAllEntrys();

            bool artefactAssemblyFound = false;
            bool artefactAssemblySymFound = false;
            for (int i = 0; i < entrys.Length; i++)
            {
                if (entrys[i].IsDir)
                    continue;

                if (entrys[i].Location == AssemblyArtefactName)
                    artefactAssemblyFound = true;
                if (entrys[i].Location == AssemblyArtefactNameSym)
                    artefactAssemblySymFound = true;

                result.AddArtefact(entrys[i].Location, zipArch.GetContentRaw(entrys[i].Location));
            }

            zipArch.Close();

            result.AssemblyArtefactName = AssemblyArtefactName;
            result.AssemblyArtefactNameSym = artefactAssemblySymFound ? AssemblyArtefactNameSym : null;

            result.AddedAt = DateTime.UtcNow;
            result.VersionTag = versionTag;

            if (!artefactAssemblyFound)
            {
                logger.Error(" BuildArtifacts:FromZipArchive assembly not found in zip : {0}", AssemblyArtefactName);
                return null;
            }

            return result;
        }
        public static BuildArtifacts FromDirectory(string AssemblyArtefactAbsPath, string versionTag)
        {
            BuildArtifacts result = new BuildArtifacts();
            string Dir = System.IO.Path.GetDirectoryName(AssemblyArtefactAbsPath);
            string assemblyAbsSym = pathWithoutExtension(AssemblyArtefactAbsPath);

            if (File.Exists(assemblyAbsSym + ".pdb"))
            {
                assemblyAbsSym = assemblyAbsSym + ".pdb";
            }
            else assemblyAbsSym = null;
            string[] Files = System.IO.Directory.GetFiles(Dir);

            bool artefactAssemblyFound = false;
            for (int i = 0; i < Files.Length; i++)
            {
                if (Files[i] == AssemblyArtefactAbsPath)
                    artefactAssemblyFound = true;

                result.AddArtefact(System.IO.Path.GetFileName(Files[i]), File.ReadAllBytes(Files[i]));
            }

            result.AssemblyArtefactName = System.IO.Path.GetFileName(AssemblyArtefactAbsPath);
            result.AssemblyArtefactNameSym = assemblyAbsSym == null ? null : System.IO.Path.GetFileName(assemblyAbsSym);

            result.AddedAt = DateTime.UtcNow;
            result.VersionTag = versionTag;

            if (!artefactAssemblyFound)
            {
                logger.Error(" BuildArtifacts:FromDirectory container ready but assembly not found in filesys: {0}", AssemblyArtefactAbsPath);
                return null;
            }

            return result;
        }

        static string pathWithoutExtension(string path)
        {
            int index = path.LastIndexOf('.');
            if (index >= 0)
            {
                return path.Remove(index, path.Length - index);
            }
            return path;
        }
    }

}
