using System;
using System.Collections.Generic;
using System.Text;
using TaskUniversum.Statistics;

namespace TaskBroker.Statistics
{
    public class StatMatchModel
    {
        StatRange[] currentRanges;
        StatRange[] lastRanges;

        FlushCB flushCallback;

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
        public StatMatchModel()
        {

        }
        public void checkExpired()
        {
            lock (currentRanges)
                for (int i = 0; i < currentRanges.Length; i++)
                {
                    StatRange r = currentRanges[i];
                    if (r.Expired)
                    {
                        // flush and create new
                        if (flushCallback != null)
                            flushCallback(r, this.MatchData);
                        lastRanges[i] = r;
                        r = currentRanges[i] = new StatRange(r);
                    }
                }
        }
        public void inc()
        {
            lock (currentRanges)
                for (int i = 0; i < currentRanges.Length; i++)
                {
                    StatRange r = currentRanges[i];
                    if (r.Expired)
                    {
                        // flush and create new
                        if (flushCallback != null)
                            flushCallback(r, this.MatchData);
                        lastRanges[i] = r;
                        r = currentRanges[i] = new StatRange(r);
                    }
                    r.inc();
                }
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
                sb.AppendFormat("{0}/{1:.0}/{2:.0}|", currentRanges[i].Counter, currentRanges[i].PerMinute, currentRanges[i].PerSecond);
            }
            return sb.ToString();
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
