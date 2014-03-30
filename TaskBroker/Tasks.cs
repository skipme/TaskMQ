using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;
using TaskScheduler;
using TaskUniversum;

namespace TaskBroker
{
    public class QueueTask : TaskScheduler.PlanItem 
    {
        public string ChannelName;
        public ChannelAnteroom Anteroom;

        public Dictionary<string, object> Parameters;
        public IBrokerModule Module;
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
