using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskClient
{
    public class Validation
    {
        public class ValidationRequest
        {
            public string MType { get; set; }
            public string ChannelName { get; set; }
        }
        public class ValidationResponse
        {
            public Dictionary<string, TaskQueue.RepresentedModelValue> ModelScheme { get; set; }
        }
    }
}
