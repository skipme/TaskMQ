using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Configuration
{

    public class RepresentedConfiguration
    {


        public cConnection[] Connections { get; set; }
        public cChannel[] Channels { get; set; }
        public cTask[] Tasks { get; set; }
        public cModule[] Modules { get; set; }
    }
    public class cModules
    {
        public string Name { get; set; }
        public cModel messageModel { get; set; }
    }
    public class cTask
    {
        public string Description { get; set; }
        public TaskScheduler.IntervalType intervalType { get; set; }
        public int intervalValue { get; set; }
        public TaskQueue.Providers.TItemModel parameters { get; set; }// equal with json object if configuration in json...

        public string ChannelName { get; set; }
        public string ModuleName { get; set; }

    }
    public class cChannel
    {
        public string Name { get; set; }
        public string connectionName { get; set; }
        public cModel messageModel { get; set; }
    }
    public class cModel
    {
        // assembly key required |(module id)|
        // 
        public string TypeFullName { get; set; }
    }
    public class cModule
    {
        // assembly key required |(module id)|
        // 
        public string TypeFullName { get; set; }
        // module descriptors ... version ... paths ... build system ids ... scm id ...
    }
    public class cConnection
    {
        public string Name { get; set; }
        public string connectionString { get; set; }
        public string collection { get; set; }
        public string database { get; set; }
        public string queueTypeName { get; set; }
    }
}
