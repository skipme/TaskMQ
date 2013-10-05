#undef MONO
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
    public class RepresentedConfiguration
    {
        public RepresentedConfiguration()
        {
            CreationDate = DateTime.Now;// server time...
        }
        public DateTime CreationDate { get; set; }

        public byte[] SerialiseJson(Encoding enc = null)
        {
            if (enc == null)
                enc = Encoding.UTF8;
            string v = SerialiseJsonString(enc);

            return enc.GetBytes(v);
        }
        public string SerialiseJsonString(Encoding enc = null)
        {
            if (enc == null)
                enc = Encoding.UTF8;

            string v = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            //string v = ServiceStack.Text.JsonSerializer.SerializeToString(this, this.GetType());
            //v = ServiceStack.Text.JsvFormatter.Format(v); not clever!
            return v;
        }

        public static T DeSerialiseJson<T>(string data)
           where T : RepresentedConfiguration
        {

#if MONO
            // json.net use biginteger parse method what not implemented in mono runtime
            T obj = ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(data);
#else
            T obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
#endif
            return obj;
        }
    }
    public class ConfigurationBroker : RepresentedConfiguration
    {
        public cConnection[] Connections { get; set; }
        public cChannel[] Channels { get; set; }
        public cTask[] Tasks { get; set; }

        public static ConfigurationBroker DeSerialiseJson(string json)
        {
            ConfigurationBroker obj = RepresentedConfiguration.DeSerialiseJson<ConfigurationBroker>(json);
            return obj;
        }
    }
    public class ConfigurationModules : RepresentedConfiguration
    {
        public cModule[] Modules { get; set; }

        public static ConfigurationModules DeSerialiseJson(string json)
        {
            ConfigurationModules obj = RepresentedConfiguration.DeSerialiseJson<ConfigurationModules>(json);
            return obj;
        }
    }
    public class ConfigurationAssemblys : RepresentedConfiguration
    {
        public cAssembly[] Assemblys { get; set; }

        public static ConfigurationAssemblys DeSerialiseJson(string json)
        {
            ConfigurationAssemblys obj = RepresentedConfiguration.DeSerialiseJson<ConfigurationAssemblys>(json);
            return obj;
        }
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
        public bool Auto { get; set; }
    }
    [Serializable]
    public class cChannel
    {
        public string Name { get; set; }
        public string connectionName { get; set; }
        //public string MType { get; set; }
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
        public string Description { get; set; }
        public TaskBroker.ExecutionType Role { get; set; }
        public string TypeFullName { get; set; }
        public Dictionary<string, string> ParametersModel { get; set; }
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
    [Serializable]
    public class cAssembly
    {
        public string Name { get; set; }
        public string RepositoryUrl { get; set; }// null if !useSCM
        public bool useSCM { get; set; }// clone and update from remote repo
    }
}
