using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.Containers
{
    /// <summary>
    /// PackageInfo ref meta+ Container
    /// </summary>
    public class AssemblyVersionPackage
    {
        public AssemblyVersionPackage(Ref.AssemblyArtifacts meta, Containers.AssemblyBinVersions archive)
        {
            Version = meta;
            Container = archive;
        }
        public readonly Ref.AssemblyArtifacts Version;
        private readonly Containers.AssemblyBinVersions Container;
        public string ContainerName
        {
            get
            {
                return Container.name;
            }
        }
        public Ref.AssemblyArtifact FindArtefactByName(string fileName)
        {
            var art = (from a in Version.Artefacts
                       where a.FileName == fileName
                       select a).FirstOrDefault();
            return art;
        }
        public byte[] ExtractArtefact(Ref.AssemblyArtifact art)
        {
            int i;
            Build.BuildResultFile res;
            if ((i = Version.Artefacts.IndexOf(art)) < 0)
                throw new Exception("artefact not present");

            if (Container.GetSpecificVersionArtefact(Version.VersionTag, art.FileName, out res))
            {

            }
            else
            {
                Console.WriteLine("error while artefact extraction");
                return null;
            }
            return res.Data;
        }
        public byte[] ExtractLibrary()
        {
            Build.BuildResultFile res;
            if (Container.GetSpecificVersionArtefact(Version.VersionTag, Version.FileLibarary, out res))
            {

            }
            else
            {
                Console.WriteLine("error while artefact extraction");
                return null;
            }
            return res.Data;
        }
        public byte[] ExtractLibrarySymbols()
        {
            Build.BuildResultFile res;

            if (Container.GetSpecificVersionArtefact(Version.VersionTag, Version.FileSymbols, out res))
            {

            }
            else
            {
                Console.WriteLine("error while artefact extraction");
                return null;
            }
            return res.Data;
        }
    }
}
