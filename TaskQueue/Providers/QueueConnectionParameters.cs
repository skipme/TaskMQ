using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue.Providers
{
    public class QueueConnectionParameters
    {
        public string Name { get; set; }
        /// <summary>
        /// table name
        /// </summary>

        public string Collection { get; set; }
        public string Database { get; set; }
        public string ConnectionString { get; set; }

        public string QueueTypeName { get; set; }
        public TaskQueue.ITQueue QueueInstance { get; set; }
    }
}
