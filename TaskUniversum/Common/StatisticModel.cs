using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum.Statistics
{
    public interface IPersistRange
    {
        int Counter { get; }
        DateTime Left { get; }
    }

    public class StatRange
    {
        public StatRange(StatRange prev)
            : this(prev.secondsInterval, prev.NextLeft)
        {
        }
        public StatRange(int secondsRange)
        {
            this.secondsInterval = secondsRange;
            this.Left = DateTime.UtcNow;
            this.NextLeft = this.Left.AddSeconds(secondsInterval);
        }
        public StatRange(int secondsRange, DateTime Left)
        {
            this.secondsInterval = secondsRange;
            this.Left = Left;// prevLeft.AddSeconds(secondsInterval);
            this.NextLeft = Left.AddSeconds(secondsInterval);
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

        public void Restore(IPersistRange mr)
        {
            this.Counter = mr.Counter;
            this.Left = mr.Left;
        }
    }
   
}
