using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker
{
    public delegate void ProducerEntryPoint(Dictionary<string, object> parameters);
    public delegate bool ConsumerEntryPoint(Dictionary<string, object> parameters, ref TaskQueue.ITItem q_parameter);

    public enum ExecutionType
    {
        Consumer = 0x11,
        Producer
    }

    public class QueueTask
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public TaskScheduler.PlanItem Plan { get; set; }
        public ExecutionType ExecAs { get; set; }

        public string MessageType { get; set; }

        public TaskQueue.TQItemSelector consumerSelector { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public ProducerEntryPoint Producer { get; set; }
        public ConsumerEntryPoint Consumer { get; set; }

        public DateTime NextExecution
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
