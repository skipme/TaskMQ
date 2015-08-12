using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TCPBsonBase
{
    public class BsonSerialiser
    {
        static NFX.Serialization.Slim.SlimSerializer ser = new NFX.Serialization.Slim.SlimSerializer();
        public static byte[] Serialise<T>(T obj)
        {
            //System.IO.MemoryStream ms = new System.IO.MemoryStream();
            
            //ser.Serialize(ms, obj);
            //return ms.GetBuffer();
            return null;
        }
        public static T DeSerialise<T>(byte[] rawData)
        {
            //System.IO.MemoryStream ms = new System.IO.MemoryStream(rawData);

            //return (T)ser.Deserialize(ms);
            return default(T);
        }

        //public static byte[] Serialise<T>(T obj)
        //{
        //    System.IO.MemoryStream ms = new System.IO.MemoryStream();
        //    ProtoBuf.Serializer.Serialize<T>(ms, obj);
        //    //IStateObj

        //    return ms.GetBuffer();
        //}
        //public static T DeSerialise<T>(byte[] rawData)
        //{
        //    System.IO.MemoryStream ms = new System.IO.MemoryStream(rawData);

        //    T result = ProtoBuf.Serializer.Deserialize<T>(ms);
        //    return result;
        //}

        //public static byte[] Serialise(object obj)
        //{
        //    System.IO.MemoryStream ms = new System.IO.MemoryStream();
        //    Newtonsoft.Json.Bson.BsonWriter bw = new Newtonsoft.Json.Bson.BsonWriter(ms);
        //    Newtonsoft.Json.JsonSerializer ser = Newtonsoft.Json.JsonSerializer.Create();
        //    ser.Serialize(bw, obj);
        //    //IStateObj

        //    return ms.GetBuffer();
        //}
        //public static T DeSerialise<T>(byte[] rawData)
        //{
        //    System.IO.MemoryStream ms = new System.IO.MemoryStream(rawData);
        //    Newtonsoft.Json.Bson.BsonReader br = new Newtonsoft.Json.Bson.BsonReader(ms);
        //    Newtonsoft.Json.JsonSerializer ser = Newtonsoft.Json.JsonSerializer.Create();
        //    T result = ser.Deserialize<T>(br);
        //    return result;
        //}
    }
}
