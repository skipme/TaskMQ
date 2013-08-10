using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Statistics
{
    public class StatHub
    {
        public static int[] useRanges = new int[] { StatRange.seconds30, StatRange.min, StatRange.min30, StatRange.hour2 };

        public void FlushReatairedChunks()
        {
            foreach (StatMatchModel m in RetrievedModels)
            {
                m.checkExpired();
            }
            // save all to persistent component
        }
        public List<StatMatchModel> RetrievedModels = new List<StatMatchModel>();

        public T FindModel<T>(T match)
            where T : StatMatchModel
        {
            // restore from persistent component
            match.CreateRanges(useRanges);
            RetrievedModels.Add(match);
            return match;
        }
    }
}
