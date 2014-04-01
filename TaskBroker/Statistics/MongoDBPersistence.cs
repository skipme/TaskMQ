using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Statistics
{
    public class MongoRange : TaskUniversum.Statistics.IPersistRange
    {
        public MongoRange()
        {
        }

        [BsonExtraElements]
        public Dictionary<string, object> MatchElements { get; set; }

        public int Counter { get; set; }
        public DateTime Left { get; set; }
        public int SecondsInterval { get; set; }

        //public MongoDB.Bson.BsonObjectId id { get; set; }
    }
    public class MongoDBPersistence
    {
        public MongoDBPersistence(string conString, string dbName, string colName = "QStats")
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

        public void PurgeExcessive(int secRange, int secAlive)
        {
//            string js = @"
//var date = new Date(); 
//date.setSeconds(date.getSeconds()- {0});
//db.{2}.remove({SecondsInterval: {1}, Left:{$lt:date}})
//";
            // 0 - seconds life of significant chunks
            // 1 - chunk id...
            // 2 - collection
            DateTime dtl = DateTime.UtcNow.AddSeconds(-secAlive);

            CheckConnection();
            Collection.Remove(Query.And(
                Query<MongoRange>.EQ(p => p.SecondsInterval, secRange),
                Query<MongoRange>.LT(p => p.Left, dtl)
            ));
        }
        public void EnsureIndex(Dictionary<string, object> matchData)
        {
            // matchKeys, Interval, Left
            IndexKeysBuilder ikb = new IndexKeysBuilder();
            ikb = ikb.Ascending(matchData.Keys.ToArray());

            CheckConnection();
            Collection.EnsureIndex(ikb);
            //
            var indexE = IndexKeys<MongoRange>.Ascending(r => r.SecondsInterval).Ascending(r => r.Left);
            Collection.EnsureIndex(indexE);
        }
        private IMongoQuery GetQuery(Dictionary<string, object> matchData)
        {
            List<IMongoQuery> qs = new List<IMongoQuery>();
            foreach (KeyValuePair<string, object> kv in matchData)
            {
                qs.Add(Query.EQ(kv.Key, (string)kv.Value));
            }
            return Query.And(qs);
        }
        public IEnumerable<MongoRange> GetNewest(Dictionary<string, object> matchData)
        {
            /*
             * 
db.tmqStats.aggregate({$match:{ch:"4", z:"5"}}, { $sort: { Left: 1 } }, 
{$group:{_id:"$SecondsInterval", Left:{$last:"$Left"}, id:{$last:"$_id"}, Counter:{$last:"$Counter"}}}, 
{$project:{_id:"$id",SecondsInterval:"$_id", Left:1, Counter:1}})
             * 
             */

            var match = new BsonDocument
			{
				{
					"$match",
					new BsonDocument(matchData)
				}
			};
            var sort = new BsonDocument
			{
				{
					"$sort",
					new BsonDocument("Left", 1)
				}
			};
            var group = new BsonDocument
			{
				{
					"$group",
					new BsonDocument
						{
												{
													"_id","$SecondsInterval"
												},
												{
													"Left", new BsonDocument
																{
																	{ "$last", "$Left" }
																}
												},
												{
													"id", new BsonDocument
																{
																	{ "$last", "$_id" }
																}
												},
												{
													"Counter", new BsonDocument
																{
																	{ "$last", "$Counter" }
																}
												}
											}
				}
			};

            var project = new BsonDocument
			{
				{
					"$project",
					new BsonDocument
						{
							//{"_id","$id"},
							{"_id",0},
							{"SecondsInterval","$_id"}, {"Left",1}, {"Counter",1}
						}
				}
			};
            var pipeline = new[] { match, sort, group, project };

            CheckConnection();
            var result = Collection.Aggregate(pipeline);
            foreach (var item in result.ResultDocuments)
            {
                //item.Add(matchData);
                yield return MongoDB.Bson.Serialization.BsonSerializer.Deserialize<MongoRange>(item);
            }
        }
        public void Save(MongoRange range)
        {
            // insert/update
            var query = Query.And(GetQuery(range.MatchElements), Query<MongoRange>.EQ(p => p.Left, range.Left));

            CheckConnection();
            Collection.Update(query,
                Update<MongoRange>.Set(p => p.Left, range.Left)
                .Set(p => p.Counter, range.Counter).Set(p => p.SecondsInterval, range.SecondsInterval)
                //.Set(p => p.MatchElements, range.MatchElements)
                ,
                UpdateFlags.Upsert,
                WriteConcern.Acknowledged);
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
