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
        public AssemblyVersionPackage(Ref.PackageVersion meta, Containers.AssemblyBinVersions archive)
        {
            Version = meta;
            Container = archive;
        }
        public readonly Ref.PackageVersion Version;
        private readonly Containers.AssemblyBinVersions Container;
        public string ContainerName
        {
            get
            {
                return Container.name;
            }
        }
        public Ref.PackageVersionArtefact FindArtefactByName(string fileName)
        {
            var art = (from a in Version.Artefacts
                       where a.File == fileName
                       select a).FirstOrDefault();
            return art;
        }
        public byte[] ExtractArtefact(Ref.PackageVersionArtefact art)
        {
            int i;
            Build.BuildResultFile res;
            if ((i = Version.Artefacts.IndexOf(art)) < 0)
                throw new Exception("artefact not present");

            if (Container.GetSpecificVersionArtefact(Version.Tag, art.File, out res))
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
            if (Container.GetSpecificVersionArtefact(Version.Tag, Version.FileLibarary, out res))
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

            if (Container.GetSpecificVersionArtefact(Version.Tag, Version.FileSymbols, out res))
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
