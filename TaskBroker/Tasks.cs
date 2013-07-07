using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;
using TaskScheduler;

namespace TaskBroker
{

    public enum ExecutionType
    {
        Consumer = 0x11,
        Producer
    }

    public class ModuleSelfTask
    {
        public string ChannelName;
        public string NameAndDescription;
        public string ModuleName { get; set; }
        public IntervalType intervalType { get; set; }
        public long intervalValue { get; set; }
    }
    public class QueueTask : TaskScheduler.PlanItem 
    {
        public string Description;

        public string ChannelName;
        public bool MinChannelOccupancyDirection;
        public TItemModel Parameters;
        public ModMod Module;
        public string ModuleName { get; set; }

        public bool Temp;

        public DateTime NextExecution
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
