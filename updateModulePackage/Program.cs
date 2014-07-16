using SourceControl.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace updateModulePackage
{
    class Program
    {
        /// <summary>
        /// scm -> build -> package => platform
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        public static void Main(string[] args)
        {
            string scmUrl = "";// git only now
            string scmProjectPath = "";
            string moduleName = "";

            if (args[0] == "artifacts")
            {
                string rev = Guid.NewGuid().ToString();
                SourceControl.BuildServers.BuildArtifacts bas = SourceControl.BuildServers.BuildArtifacts.FromDirectory(args[1], rev);
                AssemblyBinVersions Versions = new AssemblyBinVersions(System.IO.Directory.GetCurrentDirectory(), args[2]);
                Versions.AddVersion(new SourceControl.Ref.SCMRevision()
                {
                    Commiter = "updateModulePackage tool",
                    CommitTime = DateTime.UtcNow,
                    CreateAt = DateTime.UtcNow,
                    Revision = rev
                }, bas);
                return;
            }
            if (args.Length != 3)
            {
                Console.WriteLine("not enough parameters, and make sure it in following sequence: ");
                Console.WriteLine("scmURL scmRelativeProjectPath moduleName");
                Console.ReadLine();
                return;
            }
            else
            {
               
                scmUrl = args[0];
                scmProjectPath = args[1];
                moduleName = args[2];
            }

            SourceControl.BuildServers.NaiveMSfromGit git = new SourceControl.BuildServers.NaiveMSfromGit();
            git.SetParameters(new SourceControl.BuildServers.NaiveMSfromGitParams()
            {
                SCM_URL = scmUrl,
                ProjectPath = scmProjectPath,
                AssemblyPath = null
            });
            SourceControl.Assemblys.SourceController prj = new SourceControl.Assemblys.SourceController(System.IO.Directory.GetCurrentDirectory(), moduleName, git);

            prj.Fetch();
            prj.Build();
            prj.UpdatePackage();
            //bool sourceUpdateResult = prj.SetUpSourceToDate();
            //if (sourceUpdateResult)
            //{
            //    string bl = "";
            //    bool buildResult = prj.StoreNewIfRequired(out bl);
            //    if (!buildResult)
            //        Console.WriteLine("build error: {0}", bl);
            //    else
            //        Console.WriteLine("ok.");
            //}
            //else
            //{
            //    Console.WriteLine("   ");
            //    Console.WriteLine("some errors occure, checkout log.");
            //}
            Console.ReadLine();
        }
    }
}
