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
    // utilisation between module interface and broker
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
        public string ChannelName;
        public ChannelAnteroom Anteroom;

        public Dictionary<string, object> Parameters;
        public ModMod Module;
        public string ModuleName { get; set; }
        public bool Temp; // remove after restart/reset (this task used for autoconfigure and autoscale)
        
        // automatically change interval(0ms-60sec) by service a queue (away from blocking lag)
        // can be used it - if you don't know how a queue must serviced
        public bool ExecutionPlanSpecifiedByChannel;

        public DateTime NextExecution
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
