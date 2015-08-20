using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BenchWriter
{
    class Program
    {
        const int BATCH_SIZE = 32;
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
            Collection<Dictionary<string, object>> batch = new Collection<Dictionary<string, object>>();
            for (int i = 0; i < BATCH_SIZE; i++)
			{
                batch.Add(msg);
			}
            
            while (true)
            {
                //restcli.Enqueue(msg);
                //tcpb.Enqueue(msg);
                tcpb.ApiEnqueueBatch(batch);
                System.Threading.Thread.Sleep(0);
                //System.Threading.Thread.Sleep(1000*15);
            }
            Console.Read();
        }
    }
}
