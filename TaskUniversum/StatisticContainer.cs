using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum.Statistics
{
    public struct MetaStatRange
    {
        public string Name;
        public StatRange range;
    }
    public class StatisticContainer
    {
        public List<MetaStatRange> FlushedMinRanges = new List<MetaStatRange>();
    }

}
