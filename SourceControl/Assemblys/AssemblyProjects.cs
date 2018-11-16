using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskUniversum;

namespace SourceControl.BuildServers
{
    public class AssemblyProjects
    {
        static ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        public string DirectoryBase { get; set; }
        public List<SourceController> hostedProjects;
        public BSDepot BuildServersProvider;

        public AssemblyProjects(string path)
        {
            DirectoryBase = path;
            CheckDirectory();

            hostedProjects = new List<SourceController>();
            BuildServersProvider = new BSDepot();
        }
        public void AddPackage(string name, string buildServerType, Dictionary<string, object> parameters)
        {
            BuildServers.IBuildServer bs = BuildServersProvider.GetNewInstance(buildServerType);
            if (bs == null)
            {
                logger.Error("Build server type not found: \"{0}\" for assembly spec: \"{1}\" ", buildServerType, name);
                return;
            }
            bs.SetParameters(parameters);

            hostedProjects.Add(new SourceController(DirectoryBase, name, bs));
        }
        public IEnumerable<SourceController> TakeLoadTime()
        {
            foreach (SourceController p in hostedProjects.OrderBy(o => o.Versions.LastPackagedDate))
            {
                yield return p;
            }
        }
        private void CheckDirectory()
        {
            if (!System.IO.Directory.Exists(DirectoryBase))
            {
                System.IO.Directory.CreateDirectory(DirectoryBase);
            }
        }
    }
}
