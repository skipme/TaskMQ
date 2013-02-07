using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TApp
{
    class Program
    {


        static void Main(string[] args)
        {
            

            TaskBroker.Broker b = new TaskBroker.Broker();
            QueueService.ModProducer.Initialise(b);

            Console.Read();
        }
    }
}
