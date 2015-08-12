using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPBsonBase
{
    public class StateObj
    {
        public string state;

        public Dictionary<string, object> stateParameters;

        public int PAsInt(string name)
        {
            long rp = (long)stateParameters[name];
            unchecked
            {
                return (int)rp;
            }
        }
        public Dictionary<string, object> PAsDict(string name)
        {
            Newtonsoft.Json.Linq.JObject rp = (Newtonsoft.Json.Linq.JObject)stateParameters[name];


            return rp.ToObject<Dictionary<string, object>>();
        }
        public byte[] GetBsonData()
        {
            return BsonSerialiser.Serialise(this);
        }
        public static T GetStateObj<T>(byte[] bsonData) where T : StateObj
        {
            T stobj = BsonSerialiser.DeSerialise<T>(bsonData);

            return stobj;
        }

    }
}
