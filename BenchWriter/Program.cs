using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenchWriter
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskClient.Clients.HttpRest restcli = new TaskClient.Clients.HttpRest();
            var msg = new Dictionary<string, object>
                {
                    {"MType", "benchMessage"},
                    /*
                    {"ParameterA", null}, // DATA WITH NULL NOT TRANSFERRED
                    {"ParameterB", null}*/
                    {"ParameterA", "unset"},
                    {"ParameterB", "unset"}
                };
            while (true)
            {
                restcli.Enqueue(msg);
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
