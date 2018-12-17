using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    using NUnit.Framework;
    using System.IO;

    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using Microsoft.CSharp;
    using System.Reflection;


    [TestFixture]
    public class AssemblyTests
    {
        const string dir_with_libs = "test_libs";
        public AssemblyTests()
        {
            if (!Directory.Exists(dir_with_libs))
                Directory.CreateDirectory(dir_with_libs);
            ClearWorkDir();
        }
        static void ClearWorkDir()
        {
            foreach (string fl in Directory.EnumerateFiles(dir_with_libs))
            {
                File.Delete(fl);
            }
        }
        static string CreateLib(string source, string location, string[] dll_references = null)
        {
            location = Path.Combine(dir_with_libs, location);

            if (File.Exists(location))
                File.Delete(location);

            CSharpCodeProvider codeProvider = new CSharpCodeProvider();

            System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();

            if (dll_references != null)
            {
                parameters.ReferencedAssemblies.AddRange(dll_references);
            }
            else
            {
                parameters.ReferencedAssemblies.Add(typeof(TaskQueue.ITItem).Assembly.Location);
                parameters.ReferencedAssemblies.Add(typeof(TaskUniversum.IModConsumer).Assembly.Location);
            }
            parameters.GenerateExecutable = false;
            parameters.OutputAssembly = location;

            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, source);

            if (results.Errors.Count > 0)
                throw new Exception("can't compile library...");

            return location;
        }
        static string CreateAsset(string location, string content)
        {
            location = Path.Combine(dir_with_libs, location);
            File.WriteAllText(location, content, Encoding.ASCII);
            return location;
        }
        [Test]
        public void JustAssemblyImport()
        {
            ClearWorkDir();
            const string lib_source = @"
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle(""%libname%"")]

namespace lib_a
{
public class type_class_a
{
public int do_something()
{
System.Int32 x = 33;
return x * (int)System.DateTime.Now.Millisecond * 0 + 55741;
}
}
}

";
            const string lib_name = "lib_a_1";
            string lib_location = lib_name + ".dll";

            const string asset_name = "info.txt";
            string asset_location = asset_name;
            string asset_content = "hello there";

            lib_location = CreateLib(lib_source.Replace("%libname%", lib_name), lib_location);
            asset_location = CreateAsset(asset_location, asset_content);// asset created in same dir as lib, it will appear in package

            TaskBroker.Broker br = new TaskBroker.Broker();

            SourceControl.BuildServers.LocalDirectory lds = new SourceControl.BuildServers.LocalDirectory();
            SourceControl.BuildServers.LocalDirParams lds_p = new SourceControl.BuildServers.LocalDirParams()
            {
                AssemblyFileName = lib_location
            };

            if (File.Exists(lib_name + ".zip"))
                File.Delete(lib_name + ".zip");

            br.AddAssembly(lib_name, lds.Name, lds_p.GetHolder());
            br.PrepareBroker(false, true, false);

            Assert.True(br.AssemblyHolder.loadedAssemblys.Count == 1);
            Assert.True(br.AssemblyHolder.loadedAssemblys[br.AssemblyHolder.loadedAssemblys.Keys.First()].AssemblyName.StartsWith(lib_name));

            Type type_class_a = br.AssemblyHolder.loadedAssemblys[br.AssemblyHolder.loadedAssemblys.Keys.First()].assembly.GetExportedTypes()[0];

            object class_obj = Activator.CreateInstance(type_class_a);

            MethodInfo do_something = type_class_a.GetMethod("do_something");
            object result_of_something = do_something.Invoke(class_obj, null);

            Assert.True((int)result_of_something == 55741);

            SourceControl.Containers.AssemblyPackageVersionHelper package = br.AssemblyHolder.SharedManagedLibraries.ResolvePackage(type_class_a.Assembly.FullName);
            Assert.False(package == null);

            SourceControl.Ref.PackageInfoArtifact asset_art = package.FindArtefactByName(asset_name);
            Assert.False(asset_art == null);

            byte[] asset_data = package.ExtractArtefact(asset_art);
            string asset_check_content = Encoding.ASCII.GetString(asset_data);

            Assert.True(string.Equals(asset_check_content, asset_content, StringComparison.Ordinal));
        }

        [Test]
        public void RestrictionsByPlatfromInterfaces()
        {
            ClearWorkDir();
            const string lib_source = @"
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TaskUniversum.Task;

[assembly: AssemblyTitle(""%libname%"")]

namespace lib_a
{
public class type_class_a :ExtraTask
{
public void Enter(){MetaTask mt = new MetaTask();mt.Exit();}
}
}

";
            const string restr_lib_source = @"
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle(""%libname%"")]
[assembly: AssemblyVersion(""99.8.8"")]
[assembly: AssemblyFileVersion(""99.8.8"")]
[assembly: AssemblyKeyFileAttribute(""testkeypair.snk"")]

namespace TaskUniversum.Task
{  
public class ExtraTask
    {
       public void Exit(){xxx.some_proc();}
    }
  public class MetaTask
    {
       public void Exit(){xxx.some_proc();}
    }
public class xxx
{
public static void some_proc(){Console.WriteLine(""xxx"");}
}
}

";
            const string lib_name = "lib_a_1";
            string lib_location = lib_name + ".dll";

            string restr_lib_name = typeof(TaskUniversum.IModConsumer).Assembly.GetName().Name;
            string restr_lib_location = restr_lib_name + ".dll";

            restr_lib_location = CreateLib(restr_lib_source.Replace("%libname%", restr_lib_name), restr_lib_location);

            lib_location = CreateLib(lib_source.Replace("%libname%", lib_name), lib_location, new string[] { 
                restr_lib_location,
                //typeof(TaskQueue.ITItem).Assembly.Location,
                //typeof(TaskUniversum.IModConsumer).Assembly.Location
            });

            TaskBroker.Broker br = new TaskBroker.Broker();

            SourceControl.BuildServers.LocalDirectory lds = new SourceControl.BuildServers.LocalDirectory();
            SourceControl.BuildServers.LocalDirParams lds_p = new SourceControl.BuildServers.LocalDirParams()
            {
                AssemblyFileName = lib_location
            };

            if (File.Exists(lib_name + ".zip"))
                File.Delete(lib_name + ".zip");

            br.AddAssembly(lib_name, lds.Name, lds_p.GetHolder());
            br.PrepareBroker(false, true, false);

            Assert.True(br.AssemblyHolder.loadedAssemblys.Count == 0);
                     
        }
    }
}
