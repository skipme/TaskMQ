using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue
{
    public interface ITQueue
    {
        void Push(ITItem item);
        ITItem GetItemFifo();
        ITItem GetItem(TQItemSelector selector);


        void InitialiseFromModel(QueueItemModel model, string collection, string connectionString);
        string QueueType { get; }
        string QueueDescription { get; }
    }
}
