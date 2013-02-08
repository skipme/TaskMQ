using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoQueue
{
    public class MongoMessage
    {
        public MongoDB.Bson.BsonObjectId id { get; set; }
        public Dictionary<string, object> Body { get; set; }
    }
}
