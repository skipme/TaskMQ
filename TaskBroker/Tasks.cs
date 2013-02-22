using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker
{
    public delegate void ProducerEntryPoint(Dictionary<string, object> parameters);
    public delegate bool ConsumerEntryPoint(Dictionary<string, object> parameters, ref TaskQueue.Providers.TaskMessage q_parameter);
    public delegate void ModInitEntryPoint(TaskBroker.Broker brokerInterface, TaskBroker.ModMod thisModule);
    public delegate void StubEntryPoint();

    public enum ExecutionType
    {
        Consumer = 0x11,
        Producer
    }

    public class QueueTask
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string ChannelName { get; set; }
        public bool MinChannelOccupancyDirection { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public ModMod Module { get; set; }

        public TaskScheduler.PlanItem Plan { get; set; }
        public bool ProducedByMod { get; set; }

        public DateTime NextExecution
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
