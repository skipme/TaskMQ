using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Statistics
{
    public class MongoRange
    {
        public MongoRange()
        {
        }

        [BsonExtraElements]
        public Dictionary<string, object> MatchElements { get; set; }

        public int Counter { get; set; }
        public DateTime Left { get; set; }
        public int SecondsInterval { get; set; }

        public MongoDB.Bson.BsonObjectId id { get; set; }
    }
    public class MongoDBPersistence
    {
        public MongoDBPersistence(string conString, string dbName, string colName = "tmqStats")
        {
            ConnectionString = conString;
            DatabaseName = dbName;
            CollectionName = colName;
        }
        MongoCollection<MongoRange> Collection;
        public bool Connected { get; set; }

        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }

        public IEnumerable<MongoRange> GetNewest(Dictionary<string, string> matchData)
        {
            return null;
        }
        public IEnumerable<MongoRange> Save(MongoRange range)
        {
            // insert/update
            return null;
        }
        private void OpenConnection()
        {
            MongoClient cli = new MongoClient(ConnectionString);
            var server = cli.GetServer();
            var db = server.GetDatabase(DatabaseName);
            Collection = db.GetCollection<MongoRange>(CollectionName);
            Connected = true;
        }

        private void CheckConnection()
        {
            if (!Connected)
            {
                OpenConnection();
            }
        }
    }
}
