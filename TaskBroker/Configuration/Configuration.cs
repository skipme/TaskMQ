using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TaskQueue;

namespace TaskBroker.Configuration
{
    [Serializable]
    public class RepresentedConfiguration
    {
        public byte[] Serialise()
        {
            string v = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);

            return Encoding.Unicode.GetBytes(v);
        }
        public static T DeSerialiseXml<T>(byte[] data)
            where T : RepresentedConfiguration
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            MemoryStream memoryStream = new MemoryStream(data);
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.Unicode);

            return xs.Deserialize(memoryStream) as T;
        }
    }
    public class ConfigurationBroker : RepresentedConfiguration
    {
        public cConnection[] Connections { get; set; }
        public cChannel[] Channels { get; set; }
        public cTask[] Tasks { get; set; }
    }
    public class ConfigurationModules : RepresentedConfiguration
    {
        public cModule[] Modules { get; set; }
    }

    [Serializable]
    public class cTask
    {
        public string Description { get; set; }
        public TaskScheduler.IntervalType intervalType { get; set; }
        public long intervalValue { get; set; }
        public Dictionary<string, object> parameters { get; set; }// equal with json object if configuration in json...

        public string ChannelName { get; set; }
        public string ModuleName { get; set; }
    }
    [Serializable]
    public class cChannel
    {
        public string Name { get; set; }
        public string connectionName { get; set; }
        public cModel messageModel { get; set; }
    }
    [Serializable]
    public class cModel
    {
        // assembly key required |(module id)|
        // 
        public string TypeFullName { get; set; }
    }
    [Serializable]
    public class cModule
    {
        public string Name { get; set; }
        public TaskBroker.ExecutionType Role { get; set; }
        public cModel messageModel { get; set; }
        // assembly key required |(module id)|
        // 
        public string TypeFullName { get; set; }
        // module descriptors ... version ... paths ... build system ids ... scm id ...
        public string AssemblyFile { get; set; }
    }
    [Serializable]
    public class cConnection
    {
        public string Name { get; set; }
        public string connectionString { get; set; }
        public string collection { get; set; }
        public string database { get; set; }
        public string queueTypeName { get; set; }
    }
}
