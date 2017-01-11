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
        //Providers.TaskMessage GetItemFifo();
        Providers.TaskMessage GetItem();
        Providers.TaskMessage[] GetItemTuple();
        long GetQueueLength();
        void UpdateItem(Providers.TaskMessage item);

        /// <summary>
        /// exception will drop any channel initialisation context!!! TODO:
        /// </summary>
        /// <param name="model"></param>
        /// <param name="connection"></param>
        void InitialiseFromModel(RepresentedModel model, QueueConnectionParameters connection);

        void SetSelector(TQItemSelector selector);

        string QueueType { get; }
        string QueueDescription { get; }

        void OptimiseForSelector();
        QueueSpecificParameters GetParametersModel();
    }
}
