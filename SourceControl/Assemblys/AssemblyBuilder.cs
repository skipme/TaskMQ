using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControl
{
    public class AssemblyBuilder
    {
        public class bLogger : Microsoft.Build.Framework.ILogger, IDisposable
        {
            StringBuilder log = new StringBuilder();
            public void Initialize(Microsoft.Build.Framework.IEventSource eventSource)
            {
                eventSource.AnyEventRaised += eventSource_AnyEventRaised;
            }

            void eventSource_AnyEventRaised(object sender, Microsoft.Build.Framework.BuildEventArgs e)
            {
                log.AppendFormat("el: [{0}] {1}: {2}", e.Timestamp, e.SenderName, e.Message);
                //Console.WriteLine("el: [{0}] {1}: {2}", e.Timestamp, e.SenderName, e.Message);
            }

            public string Parameters
            {
                get
                {
                    return "";
                }
                set
                {
                }
            }

            public void Shutdown()
            {
            }

            public Microsoft.Build.Framework.LoggerVerbosity Verbosity
            {
                get
                {
                    return Microsoft.Build.Framework.LoggerVerbosity.Minimal;
                }
                set
                {
                }
            }
            public string Result()
            {
                return log.ToString();
            }

            public void Dispose()
            {
                log = null;
            }
        }

        public string ProjectLocation { get; set; }
        public string Log;
        public string BuildResultDll;
        public string BuildResultSymbols;

        public AssemblyBuilder(string projectPath)
        {
            ProjectLocation = @"C:\Users\USER\test\EmailConsumer\EmailConsumer.csproj";
        }
        public bool BuildProject()
        {
            Log = "";
            bool result = false;
            BuildResultDll = BuildResultSymbols = "";

            try
            {
                using (bLogger logger = new bLogger())
                {
                    Project p = new Project(ProjectLocation);
                    string path = p.GetPropertyValue("TargetPath");
                    string pdb = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path) + ".pdb");

                    result = p.Build(logger);
                    Log = logger.Result();

                    if (result)
                    {
                        BuildResultDll = path;
                        BuildResultSymbols = pdb;
                    }
                }
            }
            catch (Exception e)
            {
                Log += string.Format("major exception while build: {0}", e.Message);
            }
            return result;
        }
    }
}
