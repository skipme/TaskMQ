﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace TaskQueue
{
    public interface ITQueue
    {
        void Push(ITItem item);
        ITItem GetItemFifo();
        ITItem GetItem(TQItemSelector selector);
        void UpdateItem(ITItem item);

        ITItem[] GetItemTuple(TQItemSelector selector);

        void InitialiseFromModel(QueueItemModel model, QueueConnectionParameters connection);
        string QueueType { get; }
        string QueueDescription { get; }

        void OptimiseForSelector(TQItemSelector selector);
    }
}
