using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue;
using TaskQueue.Providers;

namespace MSSQLQueue
{
    public class MsSqlQueue : ITQueue
    {

        public void Push(TaskMessage item)
        {
            throw new NotImplementedException();
        }

        public TaskMessage GetItemFifo()
        {
            throw new NotImplementedException();
        }

        public TaskMessage GetItem()
        {
            throw new NotImplementedException();
        }

        public TaskMessage[] GetItemTuple()
        {
            throw new NotImplementedException();
        }

        public long GetQueueLength()
        {
            throw new NotImplementedException();
        }

        public void UpdateItem(TaskMessage item)
        {
            throw new NotImplementedException();
        }

        public void InitialiseFromModel(RepresentedModel model, QueueConnectionParameters connection, TQItemSelector selector = null)
        {
            throw new NotImplementedException();
        }

        public string QueueType
        {
            get { return "MssqlQ"; }
        }

        public string QueueDescription
        {
            get { return "Mssql server simple queue, CollectionName is tableName, schema table generated automatically"; }
        }

        public void OptimiseForSelector()
        {
            throw new NotImplementedException();
        }
    }
}
