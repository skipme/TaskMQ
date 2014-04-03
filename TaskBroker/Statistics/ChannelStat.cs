using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskUniversum.Statistics;

namespace TaskBroker.Statistics
{
    public class BrokerStat : StatMatchModel
    {
        Dictionary<string, object> match;
        public BrokerStat(string role, string name)
        {
            this.Name = name;
            this.Role = role;
            match = new Dictionary<string, object>
            {
                {"_role", role},
                {"_name", Name}
            };
        }

        public string Name { get; private set; }
        public string Role { get; private set; }

        public override Dictionary<string, object> MatchData
        {
            get
            {
                return match;
            }
        }
    }
}
