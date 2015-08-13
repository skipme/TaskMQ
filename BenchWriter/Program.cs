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
            //TaskClient.Clients.HttpRest restcli = new TaskClient.Clients.HttpRest();
            TaskClient.Clients.TcpBson tcpb = new TaskClient.Clients.TcpBson();
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
                //restcli.Enqueue(msg);
                tcpb.Enqueue(msg);
                System.Threading.Thread.Sleep(0);
                //System.Threading.Thread.Sleep(1000*15);
            }
            Console.Read();
        }
    }
}
