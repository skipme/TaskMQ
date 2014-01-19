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

            SourceControl.Assemblys.AssemblyProject prj = new SourceControl.Assemblys.AssemblyProject(System.IO.Directory.GetCurrentDirectory(), scmProjectPath, scmUrl, moduleName);

            bool sourceUpdateResult = prj.SetUpSourceToDate();
            if (sourceUpdateResult)
            {
                string bl = "";
                bool buildResult = prj.StoreNewIfRequired(out bl);
                if (!buildResult)
                    Console.WriteLine("build error: {0}", bl);
                else
                    Console.WriteLine("ok.");
            }
            else
            {
                Console.WriteLine("   ");
                Console.WriteLine("some errors occure, checkout log.");
            }
            Console.ReadLine();
        }
    }
}
