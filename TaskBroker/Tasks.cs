using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace TaskBroker
{
    //public delegate void ProducerEntryPoint(TItemModel parameters);
    //public delegate bool ConsumerEntryPoint(TItemModel parameters, ref TaskQueue.Providers.TaskMessage q_parameter);
    //public delegate void ModInitEntryPoint(TaskBroker.Broker brokerInterface, TaskBroker.ModMod thisModule);
    //public delegate void StubEntryPoint();

    public enum ExecutionType
    {
        Consumer = 0x11,
        Producer
    }

    public class QueueTask : TaskScheduler.PlanItem 
    {
        public string Description;

        public string ChannelName;
        public bool MinChannelOccupancyDirection;
        public TItemModel Parameters;
        public ModMod Module;
        public string ModuleName { get; set; }

        public TaskScheduler.PlanItem Plan;
        //public bool ProducedByMod;

        public DateTime NextExecution
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
