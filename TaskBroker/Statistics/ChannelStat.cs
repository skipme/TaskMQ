using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Statistics
{
    public class ChannelStat : StatMatchModel
    {
        Dictionary<string, object> match;
        public ChannelStat(string name)
        {
            this.Name = name;
            match = new Dictionary<string, object>
            {
                {"_role","Channel"},
                {"_name",Name}
            };
        }

        public string Name { get; private set; }
        public override Dictionary<string, object> MatchData
        {
            get
            {
                return match;
            }
        }
    }
}
