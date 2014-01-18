using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BulkMails
{
    class Program
    {
        static void Main(string[] args)
        {
            // have some ldap catalog with 1000 persons and organisations
            // now we need to send them unique email

            // A:
            // 1. retrieve message model from platform by rest
            // 2. validate our email model with server email model

            // B:
            // 1. iterate items with email address
            // 2. send populated message(validated email model) by rest

            Stopwatch w = Stopwatch.StartNew();
            TaskClient.Clients.HttpRest rest = new TaskClient.Clients.HttpRest();
            for (int i = 0; i < 1000; i++)
            //int i = 0;
            //while(true)
            {
                i++;
                EMail mail = new EMail
                {
                    To = "example@localhost",
                    Body = "hello#" + (i + 1)
                };
                //try
                {
                    rest.Enqueue(mail);
                }
                //catch (Exception e)
                {
                    //Console.WriteLine("E: {0}", e.Message);
                }
                
            }
            w.Stop();
            Console.WriteLine("total {0}ms for 1000 items", w.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}
