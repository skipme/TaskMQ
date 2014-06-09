using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace test_RazorHandy
{
    public class Url
    {
        public static List<string> contentFiles = new List<string>();
        public static string Content(string path)
        {
            contentFiles.Add(path);
            return path;
        }
    }

    public class Class1
    {
        static void Main()
        {
            string dirsrc = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Path.Combine(dirsrc, @"..\BBQ"));
         
            using (var service = new TemplateService())
            {
                service.AddNamespace("test_RazorHandy");

                service.GetTemplate(File.ReadAllText(@".\Views\Shared\_L.cshtml"), null, "~/Views/Shared/_L.cshtml");
                var result = service.Parse(File.ReadAllText(@".\Views\bbq\Index.cshtml"), new { }, null, "page");

                Directory.SetCurrentDirectory(dirsrc);

                File.WriteAllText(@".\BBQ_static\index.html", result);

            }

        }
    }
}
