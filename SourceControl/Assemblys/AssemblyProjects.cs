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

        public AssemblyProjects(string path)
        {
            DiretoryContainer = path;
            CheckDirectory();

            hostedProjects = new List<AssemblyProject>();
        }
        public void Add(string name, string projectPath, string scmUrl)
        {
            hostedProjects.Add(new AssemblyProject(DiretoryContainer, projectPath, scmUrl, name));
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
