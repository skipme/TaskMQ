using MongoDB.Bson;
using MongoDB.Driver;
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
            BsonObjectId objid = BsonObjectId.GenerateNewId();
            MongoQueue.MongoMessage msg = null;
            if (item is TaskMessage)
            {
                msg = new MongoMessage()
                {
                    id = objid,
                    Body = (item as TaskMessage).ToDictionary()
                };
            }

            Collection.Insert(msg);
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
