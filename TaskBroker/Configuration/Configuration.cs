#undef MONO
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using TaskQueue;
using TaskUniversum;
using TaskUniversum.Common;
using TaskUniversum.Task;

// PLATFORM CONFIGURATION, not for components, modules, etc
// for bunching/orchestration components, not for components! 
namespace TaskBroker.Configuration
{
    public class RepresentedConfiguration : IRepresentedConfiguration
    {
        public RepresentedConfiguration()
        {
            CreationDate = DateTime.UtcNow;// server time...
        }
        public DateTime CreationDate { get; set; }

        public byte[] SerialiseJson(Encoding enc = null)
        {
            if (enc == null)
                enc = Encoding.UTF8;
            string v = SerialiseJsonString();

            return enc.GetBytes(v);
        }
        public string SerialiseJsonString()
        {
            string v = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);

            return v;
        }

        public static T DeSerialiseJson<T>(string data)
           where T : RepresentedConfiguration
        {

#if MONO
            // json.net use biginteger parse method that not implemented in mono runtime
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
    public class ExtraParameters : RepresentedConfiguration
    {
        public List<ExtraParametersBS> BuildServerTypes { get; set; }
    }
    public class ExtraParametersBS
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, SchemeValueSpec> ParametersModel { get; set; }
    }
    [Serializable]
    public class cTask
    {
        public string Description { get; set; }
        public IntervalType intervalType { get; set; }
        public long intervalValue { get; set; }
        public Dictionary<string, object> parameters { get; set; }// equal with json object if configuration in json...

        public string ChannelName { get; set; }
        public string ModuleName { get; set; }
        /// <summary>
        /// means internal task, if true this object must be ignored, its created by runtime configuration, not for platform
        /// </summary>
        public bool Auto { get; set; }
    }
    [Serializable]
    public class cChannel
    {
        public string Name { get; set; }
        public string connectionName { get; set; }
        public bool Auto { get; set; }
    }
    [Serializable]
    public class cModel
    {
        // assembly key required |(module id)|
        // 
        public string TypeFullName { get; set; }
    }
    [Serializable]
    public class SchemeValueSpec
    {
        public SchemeValueSpec()
        {

        }
        public SchemeValueSpec(TaskQueue.RepresentedModelValue schemeValue)
        {
            this.VType = schemeValue.VType.ToString();
            this.Description = schemeValue.Description;
            this.Required = schemeValue.Required;

            if (schemeValue.DefaultValue != null)
                this.DefaultValue = schemeValue.DefaultValue.ToString();
        }
        public string VType { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }

        public string DefaultValue { get; set; }
    }
    [Serializable]
    public class cModule
    {
        public string Name { get; set; }

        public Dictionary<string, SchemeValueSpec> ParametersModel { get; set; }
        /// <summary>
        /// Enable/Disable module - restart required
        /// </summary>
        public bool Enabled { get; set; }

        // Runtime specific, for extraction only, not for configuration!:
        public string Description { get; set; }
        public ExecutionType Role { get; set; }
        public string TypeFullName { get; set; }
        public string ParametersModelHashsum { get; set; }
        
    }
    [Serializable]
    public class cConnection
    {
        public string Name { get; set; }
        public string queueTypeName { get; set; }
        public Dictionary<string, object> QueueParameters { get; set; }
        public bool Auto { get; set; }
    }
    [Serializable]
    public class cAssembly
    {
        public string Name { get; set; }
        public string BuildServerType { get; set; }
        public Dictionary<string, object> BSParameters { get; set; }
    }
}
