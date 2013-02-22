using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaskQueue.Providers
{
    public class TaskMessage : TItemModel
    {
        public string MType { get; set; }
        public bool Processed { get; set; }
        public DateTime AddedTime { get; set; }
        public DateTime? ProcessedTime { get; set; }

        public TaskMessage(Dictionary<string, object> holder)
            : base(holder)
        {
        }
        public TaskMessage(string mtype)
        {
            MType = mtype;
        }
    }
}
