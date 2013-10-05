using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue;
using TaskQueue.Providers;

namespace MongoQueue
{
    public class MongoDbQueue : ITQueue
    {
        const int TupleSize = 16;
        MongoCollection<MongoQueue.MongoMessage> Collection;

        public void OptimiseForSelector(TQItemSelector selector)
        {
            Collection.EnsureIndex(MongoSelector.GetIndex(selector));
        }

        public long GetQueueLength(TQItemSelector selector)
        {
            var cursor = Collection.Find(MongoSelector.GetQuery(selector));
            long result = cursor.Count();
            return result;
        }

        public void Push(TaskMessage item)
        {
            CheckConnection();

            //WriteConcernResult result = Collection.Insert(new BsonDocument(item.GetHolder()), new MongoInsertOptions() { WriteConcern = new WriteConcern() { Journal = true } });
            //WriteConcernResult result = Collection.Insert(new MongoMessage { ExtraElements = item.GetHolder() }, new MongoInsertOptions() { WriteConcern = new WriteConcern() { Journal = true } });
            WriteConcernResult result = Collection.Insert(new MongoMessage { ExtraElements = item.GetHolder() });
            if (!result.Ok)
                throw new Exception("error in push to mongo queue: " + result.ToJson());
        }
        public T GetItemFifo<T>()
            where T : TaskMessage
        {
            TaskMessage tmr = GetItemFifo();
            T tmo = Activator.CreateInstance<T>();
            tmo.SetHolder(tmr.Holder);
            return tmo;
        }
        public TaskMessage GetItemFifo()
        {
            CheckConnection();

            TaskQueue.TQItemSelector selector = TaskQueue.TQItemSelector.DefaultFifoSelector;
            var cursor = Collection.Find(MongoSelector.GetQuery(selector)).SetSortOrder(MongoSelector.GetSort(selector));
            cursor.Limit = 1;
            MongoMessage mms = cursor.FirstOrDefault();
            if (mms == null)//empty
                return null;
            TaskMessage msg = new TaskMessage(mms.ExtraElements);
            msg.Holder.Add("_id", mms.id.Value);
            return msg;
        }

        public TaskMessage GetItem(TQItemSelector selector)
        {
            CheckConnection();

            var cursor = Collection.Find(MongoSelector.GetQuery(selector)).SetSortOrder(MongoSelector.GetSort(selector));
            cursor.Limit = 1;
            MongoMessage mms = cursor.FirstOrDefault();
            if (mms == null)//empty
                return null;
            TaskMessage msg = new TaskMessage(mms.ExtraElements);
            msg.Holder.Add("_id", mms.id.Value);
            return msg;
        }
        public void UpdateItem(TaskMessage item)
        {
            Dictionary<string, object> holder = item.GetHolder();
            object id = holder["_id"];
            if (id == null || !(id is ObjectId))
                throw new Exception("_id of queue element is missing");

            BsonObjectId objid = new BsonObjectId((ObjectId)id);
            holder.Remove("_id");

            MongoMessage msg = new MongoMessage
            {
                ExtraElements = holder,
                id = objid
            };

            //var result = Collection.Save(msg, new MongoInsertOptions() { WriteConcern = new WriteConcern() { Journal = true } });
            var result = Collection.Save(msg);
            if (!result.Ok)
                throw new Exception("error in update to mongo queue: " + result.ToJson());
        }
        //public void UpdateItem(TaskMessage item)
        //{
        //    Dictionary<string, object> holder = item.GetHolder();
        //    object id = holder["_id"];
        //    if (id == null || !(id is ObjectId))
        //        throw new Exception("_id of queue element is missing");
        //    BsonObjectId objid = new BsonObjectId((ObjectId)id);
        //    var doc = Collection.FindOne(Query.EQ("_id", objid));
        //    if (doc == null)
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        holder.Remove("_id");
        //        doc.ExtraElements = holder;
        //        var result = Collection.Save(doc, new MongoInsertOptions() { WriteConcern = new WriteConcern() { Journal = true } });
        //        if (!result.Ok)
        //            throw new Exception("error in update to mongo queue: " + result.ToJson());
        //    }
        //}

        public void InitialiseFromModel(RepresentedModel model, QueueConnectionParameters connection)
        {
            this.model = model;
            this.connection = connection;

            OpenConnection(connection);
        }

        private void OpenConnection(QueueConnectionParameters connection)
        {
            MongoClient cli = new MongoClient(connection.ConnectionString);
            var server = cli.GetServer();
            var db = server.GetDatabase(connection.Database);
            Collection = db.GetCollection<MongoQueue.MongoMessage>(connection.Collection);
            Connected = true;
        }

        private void CheckConnection()
        {
            if (!Connected)
            {
                OpenConnection(this.connection);
            }
        }

        public string QueueType
        {
            get { return "MongoDBQ"; }
        }

        public string QueueDescription
        {
            get { return "MongoDB queue"; }
        }

        public TaskMessage[] GetItemTuple(TQItemSelector selector)
        {
            var cursor = Collection.Find(MongoSelector.GetQuery(selector)).SetSortOrder(MongoSelector.GetSort(selector)).SetBatchSize(TupleSize);
            cursor.Limit = TupleSize;
            List<TaskMessage> tma = new List<TaskMessage>();
            foreach (var mms in cursor)
            {
                TaskMessage msg = new TaskMessage(mms.ExtraElements);
                msg.Holder.Add("_id", mms.id.Value);
                tma.Add(msg);
            }

            return tma.ToArray();
        }

        public void Purge()
        {
            CheckConnection();
            Collection.RemoveAll();
        }

        public bool Connected { get; set; }
        RepresentedModel model { get; set; }
        QueueConnectionParameters connection { get; set; }
    }
}
