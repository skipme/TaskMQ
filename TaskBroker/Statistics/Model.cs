using System;
using System.Collections.Generic;
using System.Text;
using TaskUniversum.Statistics;

namespace TaskBroker.Statistics
{
    public class StatMatchModel
    {
        readonly object rangesSync = new object();
        StatRange[] currentRanges;
        StatRange[] lastRanges;

        FlushCB flushCallback;
        TaskUniversum.ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();
        public StatMatchModel()
        {

        }

        public void CreateRanges(int[] secRanges, FlushCB f = null)
        {
            currentRanges = new StatRange[secRanges.Length];
            lastRanges = new StatRange[secRanges.Length];
            for (int i = 0; i < secRanges.Length; i++)
            {
                lastRanges[i] = currentRanges[i] = new StatRange(secRanges[i]);
            }

            flushCallback = f;
        }
        public void CreateRanges(int[] secRanges, MongoRange[] pranges, FlushCB f = null)
        {
            currentRanges = new StatRange[secRanges.Length];
            lastRanges = new StatRange[secRanges.Length];
            for (int i = 0; i < secRanges.Length; i++)
            {
                StatRange sr = new StatRange(secRanges[i]);
                for (int j = 0; j < pranges.Length; j++)
                {
                    if (pranges[j].SecondsInterval == secRanges[i])
                    {
                        sr.Restore(pranges[j]);
                        break;
                    }
                }
                lastRanges[i] = currentRanges[i] = sr;
            }

            flushCallback = f;
        }

        public void checkExpired()
        {
            lock (rangesSync)
                for (int i = 0; i < currentRanges.Length; i++)
                {
                    StatRange r = currentRanges[i];
                    FlushExpired(i, r);
                }
        }
        public void inc()
        {
            lock (rangesSync)
                for (int i = 0; i < currentRanges.Length; i++)
                {
                    StatRange r = currentRanges[i];
                    r = FlushExpired(i, r);
                    r.inc();
                }
        }

        private StatRange FlushExpired(int i, StatRange r)
        {
            if (r.Expired)
            {
                // flush and create new
                if (flushCallback != null)
                    flushCallback(r, this.MatchData);

                logger.Info("{0} {1}/{2}sec.", matchString(), r.Counter, r.secondsInterval);
                lastRanges[i] = r;
                r = currentRanges[i] = new StatRange(r);
            }
            return r;
        }

        public StatRange GetFlushedMinRange()
        {
            return lastRanges[0];
        }
        public string Print()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < currentRanges.Length; i++)
            {
                sb.AppendFormat("{3} {0}/{1:.0}/{2:.0}|", currentRanges[i].Counter, currentRanges[i].PerMinute, currentRanges[i].PerSecond, matchString());
            }
            return sb.ToString();
        }
        public string matchString()
        {
            string result = "";
            Dictionary<string, object> md = MatchData;
            foreach (KeyValuePair<string, object> item in md)
            {
                result += item.Value.ToString() + " ";
            }
            return result;
        }
        public virtual Dictionary<string, object> MatchData
        {
            get
            {
                return new Dictionary<string, object>();
            }
        }

    }
}
