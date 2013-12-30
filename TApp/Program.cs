using MongoQueue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TaskBroker;
using TaskBroker.Configuration;
using TaskScheduler;


namespace TApp
{
    class Program
    {
        static ManualResetEvent sync;
        //netsh http add urlacl url=http://+:82/ user=User
        static void RestartAsAdmin()
        {
            var startInfo = new ProcessStartInfo("TApp.exe") { Verb = "runas" };
            Process.Start(startInfo);
            Environment.Exit(0);
        }
        class zModel : TaskQueue.Providers.TaskMessage
        {
            public const string Name = "z";
            public zModel() : base(Name) { }
            public string SomeProperty { get; set; }
        }
        class zConsumer : IModConsumer
        {
            public bool Push(Dictionary<string, object> parameters, ref TaskQueue.Providers.TaskMessage q_parameter)
            {
                Console.WriteLine("zMessage: {0} - {1} {2}", q_parameter.GetHolder()["MType"], q_parameter.AddedTime, q_parameter.GetHolder()["_id"]);
                return true;
            }

            public void Exit()
            {

            }

            public void Initialise(Broker context, ModMod thisModule)
            {

            }

            public string Name
            {
                get { return "ZConsume"; }
            }

            public string Description
            {
                get { return null; }
            }


            public ModuleSelfTask[] RegisterTasks(ModMod thisModule)
            {
                return null;
            }


            public TaskQueue.Providers.TItemModel ParametersModel
            {
                get { return null; }
            }

            public TaskQueue.Providers.TItemModel AcceptsModel
            {
                get { return new zModel(); }
            }
        }
        static void TestStat()
        {
            TaskBroker.Statistics.StatMatchModel sm = new TaskBroker.Statistics.StatMatchModel();
            sm.CreateRanges(TaskBroker.Statistics.StatHub.useRanges);
            Random rnd = new Random(DateTime.UtcNow.Millisecond);
            while (true)
            {
                sm.inc();
                Console.Write("{0}\r", sm.Print());
                System.Threading.Thread.Sleep(10);
            }
        }
        static void TesMtStat()
        {
            TaskBroker.Statistics.MongoDBPersistence sm = new TaskBroker.Statistics.MongoDBPersistence(
                "mongodb://user:1234@localhost:27017", "Messages");
            //sm.Save(new TaskBroker.Statistics.MongoRange
            //{
            //    Counter = 6,
            //    Left = DateTime.Now,
            //    MatchElements = new Dictionary<string, object> { { "ch", "4" }, { "z", "5" } }
            //    , SecondsInterval =22
            //});
            var x = sm.GetNewest(new Dictionary<string, object> { { "ch", "4" }, { "z", "5" } }).ToList();
        }
        static void TesGit()
        {
            //SourceControl.Git.LocalBranch b = new SourceControl.Git.LocalBranch(
            //    Path.Combine(System.Environment.CurrentDirectory, "testRepo"),
            //    //"https://github.com/libgit2/TestGitRepository.git"
            //    "https://github.com/skipme/test.git"
            //    );
            //b.TakeChanges();
            //SourceControl.Assemblys.AssemblySource source = new SourceControl.Assemblys.AssemblySource(
            //   System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "scm"),
            //    "EmailConsumer/EmailConsumer.csproj", "https://github.com/skipme/TaskMQ.git");
            ////if (source.IsActualToRemote)
            //source.SetUpToDate();
            //{
            //    byte[] l = null;
            //    byte[] s = null;
            //    if (source.BuildProject(out l, out s))
            //    {
            //        Console.WriteLine("successfull build!");
            //    }
            //}
            SourceControl.Assemblys.AssemblySource src = new SourceControl.Assemblys.AssemblySource(
                System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "scm"),
                "EmailConsumer/EmailConsumer.csproj",
                //"QueueService/QueueService.csproj",
                //"SourceControl/SourceControl.csproj",
                "https://github.com/skipme/TaskMQ.git");
            SourceControl.Assemblys.AssemblyProject p = new SourceControl.Assemblys.AssemblyProject(
                System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "scm"),
                src);
            if (!p.IsSourceUpToDate)
            {
                if (p.SetUpSourceToDate())
                    p.StoreNewIfRequired();
            }
        }
        static void ZipDir()
        {
            //SourceControl.Assemblys.AssemblyBuilder a = new SourceControl.Assemblys.AssemblyBuilder("");
            //if (a.BuildProject())
            //{
            //    SourceControl.Assemblys.AssemblyBinVersions v = new SourceControl.Assemblys.AssemblyBinVersions(Path.GetFileName(a.BuildResultDll) + ".zip", Path.GetFileNameWithoutExtension(a.BuildResultDll));
            //    v.AddVersion("dbdbdbdada2343adasd54", File.ReadAllBytes(a.BuildResultDll), File.ReadAllBytes(a.BuildResultSymbols));
            //}
        }
        static void Main(string[] args)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                BrokerApplication ba = new BrokerApplication();
                ba.Run(mre);
                mre.WaitOne();
            }
            else
            {
                ApplicationKeeper.AppdomainLoop();
            }
            Console.WriteLine("Application Main entry exit.");
            Console.ReadLine();
            return;
        }
        static void CreateZBVAFromDebugDir(string path, string dll)
        {
            SourceControl.Containers.AssemblyBinVersions v = new SourceControl.Containers.AssemblyBinVersions(path, System.IO.Path.GetFileNameWithoutExtension(dll));
            string pdb = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(dll), System.IO.Path.GetFileNameWithoutExtension(dll) + ".pdb");
            List<string> files = new List<string>();
            foreach (string F in System.IO.Directory.GetFiles(path))// TODO: not recursive! not now...
            {
                if (F != dll && F != pdb && F != v.Path)
                {
                    files.Add(F);
                }
            }
            SourceControl.Build.AssemblyBinaryBuildResult r = SourceControl.Build.AssemblyBinaryBuildResult.FromFile(dll, pdb, files.ToArray());
            
            v.AddVersion(new SourceControl.Ref.VersionRevision()
                {
                    Revision = Guid.NewGuid().ToString()
                }, r);
        }
    }
}
