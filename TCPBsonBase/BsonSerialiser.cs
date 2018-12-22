﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TCPBsonBase
{
    public static class BsonSerialiser
    {
        //static NFX.Serialization.Slim.SlimSerializer ser = new NFX.Serialization.Slim.SlimSerializer();
        //public static byte[] Serialise<T>(T obj)
        //{
        //    //System.IO.MemoryStream ms = new System.IO.MemoryStream();

        //    //ser.Serialize(ms, obj);
        //    //return ms.GetBuffer();
        //    return null;
        //}
        //public static T DeSerialise<T>(byte[] rawData)
        //{
        //    //System.IO.MemoryStream ms = new System.IO.MemoryStream(rawData);

        //    //return (T)ser.Deserialize(ms);
        //    return default(T);
        //}

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

        //static Newtonsoft.Json.JsonSerializer ser = Newtonsoft.Json.JsonSerializer.Create();
        //public static byte[] Serialise(object obj)
        //{
        //    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
        //    {
        //        Newtonsoft.Json.Bson.BsonWriter bw = new Newtonsoft.Json.Bson.BsonWriter(ms);

        //        ser.Serialize(bw, obj);
        //        //IStateObj

        //        return ms.GetBuffer();
        //    }
        //}
        //public static T DeSerialise<T>(byte[] rawData)
        //{
        //    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(rawData))
        //    {
        //        Newtonsoft.Json.Bson.BsonReader br = new Newtonsoft.Json.Bson.BsonReader(ms);

        //        T result = ser.Deserialize<T>(br);
        //        return result;
        //    }
        //}
        static BsonSerialiser()
        {
            PrepareMsgPckSerialisation();
        }
        private static bool prepared_;
        public static void PrepareMsgPckSerialisation()
        {
            if (prepared_) return;
            MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(
                new MessagePack.Formatters.IMessagePackFormatter[]
                {
                    // for example, register reflection infos(can not serialize in default)
                    new MessagePack.Formatters.IgnoreFormatter<System.Reflection.MethodBase>(),
                    new MessagePack.Formatters.IgnoreFormatter<System.Reflection.MethodInfo>(),
                    new MessagePack.Formatters.IgnoreFormatter<System.Reflection.PropertyInfo>(),
                    new MessagePack.Formatters.IgnoreFormatter<System.Reflection.FieldInfo>()
                },
                new MessagePack.IFormatterResolver[]
                {
                    MessagePack.Resolvers.ContractlessStandardResolver.Instance
                });
            prepared_ = true;
        }

        public static byte[] Serialise(object obj)
        {
            return MessagePack.MessagePackSerializer.Serialize(obj);
        }
        public static T DeSerialise<T>(byte[] rawData)
        {
            return MessagePack.MessagePackSerializer.Deserialize<T>(rawData);
        }
    }
}
