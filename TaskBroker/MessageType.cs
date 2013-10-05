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

        public TaskQueue.ITQueue QueueInstance;
        public TaskQueue.TQItemSelector consumerSelector;

        public string ConnectionName { get; set; }

        public string AssignedMessageType;
        public TItemModel AssignedMessageModel;
        public string FirstModuleNameAssigned;
    }
    public class MessageType
    {
        public MessageType(TaskMessage model)
        {
            UniqueName = model.MType;
            Model = new TaskQueue.RepresentedModel(model.GetType());
        }
        public MessageType() { }
        public string UniqueName { get; set; }
        public TaskQueue.RepresentedModel Model { get; set; }
    }
}
