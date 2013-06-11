using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace TaskBroker
{
    public class MessageChannel
    {
        public MessageChannel(string uniqueName, string connectionParamsName, TaskQueue.TQItemSelector selector = null)
        {
            this.UniqueName = uniqueName;
            this.ConnectionName = connectionParamsName;
            //this.QueueName = queueName;

            if (selector == null)
                this.consumerSelector = TaskQueue.TQItemSelector.DefaultFifoSelector;
            else
                this.consumerSelector = selector;
        }
        public MessageChannel()
        {
            consumerSelector = TaskQueue.TQItemSelector.DefaultFifoSelector;
        }

        public string UniqueName { get; set; }
        public TaskQueue.Providers.TItemModel MessageModel { get; set; }
        //public string QueueName { get; set; }
        public TaskQueue.ITQueue QueueInstance { get; set; }
        public TaskQueue.TQItemSelector consumerSelector { get; set; }
        public string ConnectionName { get; set; }
    }
    public class MessageType
    {
        public MessageType(TaskMessage model)
        {
            UniqueName = model.MType;
            Model = new TaskQueue.QueueItemModel(model.GetType());
        }
        public MessageType() { }
        public string UniqueName { get; set; }
        public TaskQueue.QueueItemModel Model { get; set; }
    }
}
