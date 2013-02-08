using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue;

namespace MSSQLQueue
{
    public class MsSqlQueue : ITQueue
    {
        public void Push(ITItem item)
        {
            throw new NotImplementedException();
        }

        public ITItem GetItemFifo()
        {
            throw new NotImplementedException();
        }

        public ITItem GetItem(TQItemSelector selector)
        {
            throw new NotImplementedException();
        }

        public void UpdateItem(ITItem item)
        {
            throw new NotImplementedException();
        }

        public void InitialiseFromModel(QueueItemModel model, string collection, string connectionString)
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
    }
}
