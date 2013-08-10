using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Statistics
{
    public class ChannelStat : StatMatchModel
    {
        public ChannelStat(string name)
        {
            this.Name = name;

        }

        public string Name { get; set; }
    }
}
