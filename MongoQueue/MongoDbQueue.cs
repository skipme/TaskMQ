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
        MongoCollection<MongoQueue.MongoMessage> Collection { get; set; }

        public void Push(ITItem item)
        {
            if (item is TaskMessage)
            {
                Collection.Insert(new BsonDocument(item.GetHolder()));
            }
        }

        public ITItem GetItemFifo()
        {
            var cursor = Collection.Find(Query.EQ("Processed", false)).SetSortOrder(SortBy.Ascending("AddedTime"));
            cursor.Limit = 1;
            MongoMessage mms = cursor.FirstOrDefault();
            if (mms == null)//empty
                return null;
            TaskMessage msg = new TaskMessage(mms.ExtraElements);
            msg.Holder.Add("_id", mms.id.Value);
            return msg;
        }

        public ITItem GetItem(TQItemSelector selector)
        {
            throw new NotImplementedException();
        }

        public void UpdateItem(ITItem item)
        {
            Dictionary<string, object> holder = item.GetHolder();
            object id = holder["_id"];
            if (id == null || !(id is ObjectId))
                throw new Exception("_id of queue element is missing");
            BsonObjectId objid = new BsonObjectId((ObjectId)id);
            var doc = Collection.FindOne(Query.EQ("_id", objid));
            if (doc == null)
            {
                return;
            }
            else
            {
                doc.ExtraElements = holder;
                var result = Collection.Save(doc, new MongoInsertOptions() { WriteConcern = new WriteConcern() { Journal = true } }); ;
            }
        }

        public void InitialiseFromModel(QueueItemModel model, QueueConnectionParameters connection)
        {
            MongoClient cli = new MongoClient(connection.ConnectionString);
            var server = cli.GetServer();
            var db = server.GetDatabase(connection.Database);
            Collection = db.GetCollection<MongoQueue.MongoMessage>(connection.Collection);
        }

        public string QueueType
        {
            get { return "MongoDBQ"; }
        }

        public string QueueDescription
        {
            get { return "MongoDB queue"; }
        }
    }
}
