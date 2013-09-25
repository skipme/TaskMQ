using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SourceControl.Ref
{
    public class PackageVersionArtefact
    {
        public string Name { get; set; }
        public string File { get; set; }
        public bool IsAssembly { get; set; }
        public string Version { get; set; }
        public string HashCode { get; set; }

        //public static PackageVersionArtefact Get(byte[] data)
        //{
        //    PackageVersionArtefact art = new PackageVersionArtefact();
        //    string temp = Path.GetTempFileName();
        //    File.WriteAllBytes(temp, data);
        //    try
        //    {
        //        AssemblyName an = AssemblyName.GetAssemblyName(temp);

        //        art.Version = an.Version.ToString();
        //        art.Name = an.Name;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("I: {0}", e.Message);
        //    }
        //    File.Delete(temp);

        //    return art;
        //}
        public static PackageVersionArtefact Get(string file)
        {
            PackageVersionArtefact art = new PackageVersionArtefact();
            art.File = System.IO.Path.GetFileName(file);
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
    public class PackageVersion
    {
        public PackageVersion() { Artefacts = new List<PackageVersionArtefact>(); }
        public string Tag { get; set; }
        public string FileLibarary { get; set; }
        public string FileSymbols { get; set; }
        public string AssemblyVersion { get; set; }
        public DateTime AddedAt { get; set; }
        public List<PackageVersionArtefact> Artefacts;
        public void AddArtefact(string name, string relatedFile)
        {
            PackageVersionArtefact dllInfo = PackageVersionArtefact.Get(relatedFile);
            Artefacts.Add(dllInfo);
        }
    }
    public class PackageInfo
    {
        public PackageInfo() { Versions = new List<PackageVersion>(); }
        public string Name { get; set; }
        public List<PackageVersion> Versions;

        public PackageVersion AddRevision(string tag, string dllName, string pdbName, string dllPath)
        {
            PackageVersionArtefact dllInfo = PackageVersionArtefact.Get(dllPath);
            PackageVersion v = new PackageVersion()
            {
                Tag = tag,
                FileLibarary = dllName,
                FileSymbols = pdbName,
                AssemblyVersion = dllInfo.Version,
                AddedAt = DateTime.UtcNow
            };
            Name = dllInfo.Name;
            Versions.Add(v);
            return v;
        }
        public PackageVersion FindLatestVersion()
        {
            PackageVersion pv = (from v in Versions
                                 orderby v.AddedAt descending
                                 select v).FirstOrDefault();
            return pv;
        }
        public byte[] Serialise()
        {
            XmlSerializer xs = new XmlSerializer(typeof(PackageInfo));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings { Encoding = Encoding.Unicode, Indent = true };
                using (XmlWriter xmlTextWriter = XmlWriter.Create(memoryStream, settings))
                {
                    xs.Serialize(xmlTextWriter, this);
                }
                return memoryStream.GetBuffer();
            }
        }
        public static PackageInfo DeSerialise(byte[] data)
        {
            XmlSerializer xs = new XmlSerializer(typeof(PackageInfo));
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                return xs.Deserialize(memoryStream) as PackageInfo;
            }
        }
    }
}
