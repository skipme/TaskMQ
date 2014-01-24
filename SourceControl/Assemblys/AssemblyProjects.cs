using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl.Assemblys
{
    public class AssemblyProjects
    {
        public string DiretoryContainer { get; set; }
        public List<AssemblyProject> hostedProjects;
        public AssemblyArtifacts artifacts;

        public AssemblyProjects(string path)
        {
            DiretoryContainer = path;
            CheckDirectory();

            hostedProjects = new List<AssemblyProject>();
            artifacts = new AssemblyArtifacts();
        }
        public void Add(string name, string buildServerType, Dictionary<string, object> parameters)
        {
            BuildServers.IBuildServer bs = artifacts.GetNewInstance(buildServerType);
            bs.SetParameters(parameters);
            hostedProjects.Add(new AssemblyProject(DiretoryContainer, name, bs));
            //hostedProjects.Add(new AssemblyProject(DiretoryContainer, projectPath, scmUrl, name));
        }
        public IEnumerable<AssemblyProject> TakeLoadTime()
        {
            foreach (AssemblyProject p in hostedProjects.OrderBy(o => o.Versions.LastPackagedDate))
            {
                yield return p;
            }
        }
        public void FetchAllIfRequired()
        {
            foreach (AssemblyProject ap in hostedProjects)
            {
                ap.Fetch();
            }
        }
        public void BuildAllIfRequired()
        {
            foreach (AssemblyProject ap in hostedProjects)
            {
                if (ap.BuildDeferred)
                {
                    ap.Build();
                }
            }
        }
        public void UpdateAllIfRequired()
        {
            foreach (AssemblyProject ap in hostedProjects)
            {
                if (ap.BuildDeferred)
                {
                    ap.UpdatePackage();
                }
            }
        }
        private void CheckDirectory()
        {
            if (!System.IO.Directory.Exists(DiretoryContainer))
            {
                System.IO.Directory.CreateDirectory(DiretoryContainer);
            }
        }
    }
}
