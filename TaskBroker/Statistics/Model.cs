using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Statistics
{
    public class StatRange
    {
        public StatRange(StatRange prev)
            : this(prev.secondsInterval, prev.Left)
        {
        }
        public StatRange(int secondsRange)
        {
            secondsInterval = secondsRange;
            Left = DateTime.UtcNow;
            NextLeft = Left.AddSeconds(secondsInterval);
        }
        public StatRange(int secondsRange, DateTime prevLeft)
        {
            secondsInterval = secondsRange;
            Left = prevLeft.AddSeconds(secondsInterval);
            NextLeft = Left.AddSeconds(secondsInterval);
        }

        public const int seconds30 = 30;
        public const int min = 60;

        public const int min5 = min * 5;
        public const int min15 = min * 15;
        public const int min30 = min * 30;

        public const int hour = min * 60;
        public const int hour2 = hour * 2;
        public const int hour4 = hour * 4;
        public const int hour24 = hour * 24;

        public const int week = hour24 * 7;
        public const int month = hour24 * 31;

        public int secondsInterval { get; private set; }
        public DateTime NextLeft { get; private set; }
        public bool Expired
        {
            get
            {
                return NextLeft <= DateTime.UtcNow;
            }
        }
        public int inc()
        {
            return ++this.Counter;
        }

        public DateTime Left { get; private set; }
        public int Counter { get; private set; }
        public TimeSpan Spend
        {
            get
            {
                if (Expired)
                {
                    return TimeSpan.FromSeconds(secondsInterval);
                }
                return DateTime.UtcNow - Left;
            }
        }

        public double PerSecond
        {
            get
            {
                return (double)Counter / Spend.TotalSeconds;
            }
        }
        public double PerMinute
        {
            get
            {
                return (double)Counter / Spend.TotalMinutes;
            }
        }
    }
    public class StatMatchModel
    {
        StatRange[] currentRanges;
        StatRange[] lastRanges;
        public void CreateRanges(int[] secRanges)
        {
            currentRanges = new StatRange[secRanges.Length];
            lastRanges = new StatRange[secRanges.Length];
            for (int i = 0; i < secRanges.Length; i++)
            {
                lastRanges[i] = currentRanges[i] = new StatRange(secRanges[i]);
            }
        }

        public StatMatchModel()
        {

        }
        public void checkExpired()
        {
            for (int i = 0; i < currentRanges.Length; i++)
            {
                StatRange r = currentRanges[i];
                if (r.Expired)
                {
                    // flush and create new
                    lastRanges[i] = r;
                    r = currentRanges[i] = new StatRange(r);
                }
            }
        }
        public void inc()
        {
            for (int i = 0; i < currentRanges.Length; i++)
            {
                StatRange r = currentRanges[i];
                if (r.Expired)
                {
                    // flush and create new
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
        public virtual Dictionary<string, string> MatchData
        {
            get
            {
                return new Dictionary<string, string>();
            }
        }

    }
}
