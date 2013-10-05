using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace TaskQueue
{
    public interface ITQueue
    {
        void Push(Providers.TaskMessage item);
        Providers.TaskMessage GetItemFifo();
        Providers.TaskMessage GetItem(TQItemSelector selector);
        Providers.TaskMessage[] GetItemTuple(TQItemSelector selector);
        long GetQueueLength(TQItemSelector selector);
        void UpdateItem(Providers.TaskMessage item);

        /// <summary>
        /// exception will drop any channel initialisation context!!! TODO:
        /// </summary>
        /// <param name="model"></param>
        /// <param name="connection"></param>
        void InitialiseFromModel(RepresentedModel model, QueueConnectionParameters connection);
        string QueueType { get; }
        string QueueDescription { get; }

        void OptimiseForSelector(TQItemSelector selector);
    }
}
