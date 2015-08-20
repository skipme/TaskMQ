using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoQueue
{
    public class MongoMessage
    {
        public MongoMessage()
        {
        }

        [BsonExtraElements]
        public Dictionary<string, object> ExtraElements;

        public MongoDB.Bson.BsonObjectId id { get; set; }
    }
}
