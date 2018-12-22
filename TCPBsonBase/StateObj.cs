using System;
using System.Collections.Generic;
//using System.Collections.ObjectModel;
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
            //long rp = ((byte?)stateParameters[name]).Value;
            //unchecked
            //{
            //return (int)rp;
            //}
            return Convert.ToInt32(stateParameters[name]);
        }
        public Dictionary<string, object> PAsDict(string name)
        {
            //Newtonsoft.Json.Linq.JObject rp = (Newtonsoft.Json.Linq.JObject)stateParameters[name];

            //return rp.ToObject<Dictionary<string, object>>();
            return (stateParameters[name] as Dictionary<string, object>);
        }
        public IList<Dictionary<string, object>> PAsDictCollection(string name)
        {
            //Newtonsoft.Json.Linq.JArray rp = (Newtonsoft.Json.Linq.JArray)stateParameters[name];
            //return rp.ToObject<Collection<Dictionary<string, object>>>();
            return (from x in stateParameters[name] as object[]
                    select (from y in x as Dictionary<object, object>
                            select y
                            ).ToDictionary((ok) => ok.Key as string, (ov) => ov.Value)
                    ).ToArray();
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
