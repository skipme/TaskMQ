using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue.Providers
{
    public class QueueConnectionParameters
    {
        //Unique name for queue(important if persistant)
        public string Name { get; set; }
        /// <summary>
        /// table name
        /// </summary>

        //public string Collection { get; set; }
        //public string Database { get; set; }
        //public string ConnectionString { get; set; }

        public string QueueTypeName;
        public TaskQueue.ITQueue QueueInstance;
        public QueueSpecificConnectionParameters specParams;
    }
    public abstract class QueueSpecificConnectionParameters : TItemModel
    {
        public QueueSpecificConnectionParameters() { }
        public QueueSpecificConnectionParameters(TItemModel tm) : base(tm.GetHolder()) { }

        public abstract bool CheckParameters(out string result);
    }
}
