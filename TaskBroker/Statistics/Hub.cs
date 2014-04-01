using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskUniversum.Statistics;

namespace TaskBroker.Statistics
{
    public class StatHub
    {
        public static int[] useRanges = new int[] { StatRange.seconds30, StatRange.min, StatRange.min30, StatRange.hour2 };
        //private const int aliveCount = 20160;// 1week for 30 sec interval, 2week's for minute, 420day's for 30 min interval...
        private const int aliveCount = 2880;// 1day for 30 sec interval

        private MongoDBPersistence PersistenceChunks;
        public StatHub()
        {
            PersistenceChunks = new MongoDBPersistence("mongodb://user:1234@localhost:27017/Messages", "Messages");
            OptimisePerformance();
        }
        private void OptimisePerformance()
        {
            try
            {
                PersistenceChunks.EnsureIndex(new BrokerStat("", "").MatchData);
            }
            catch (Exception e)
            {
                Console.WriteLine("can't ensure statistic collection index");
            }
        }
        public void FlushRetairedChunks()
        {
            foreach (StatMatchModel m in RetrievedModels)
            {
                m.checkExpired();
                // save all to persistent component
            }

        }
        public void RemoveExcessiveChunks()
        {
            for (int i = 0; i < useRanges.Length; i++)
            {
                int secAlive = aliveCount * useRanges[i];
                PersistenceChunks.PurgeExcessive(useRanges[i], secAlive);
            }
            //PersistenceChunks.PurgeExcessive();
        }
        public List<StatMatchModel> RetrievedModels = new List<StatMatchModel>();

        public delegate void FlushCB(StatRange range, Dictionary<string, object> match);
        private void FlushCallBack(StatRange range, Dictionary<string, object> match)
        {
            MongoRange r = new MongoRange
            {
                Counter = range.Counter,
                Left = range.Left,
                MatchElements = match,
                SecondsInterval = range.secondsInterval
            };
            try
            {
                PersistenceChunks.Save(r);
            }
            catch (Exception e)
            {
                Console.WriteLine("can't save statistic collection index");
            }
            //Console.WriteLine("stat ch saved: {0}", match.Values.FirstOrDefault());
        }

        public BrokerStat InitialiseModel(BrokerStat instanceMatch)
        {
            return InitialiseModel<BrokerStat>(instanceMatch);
        }
        private T InitialiseModel<T>(T match)
            where T : StatMatchModel
        {
            // restore from persistent component
            MongoRange[] pranges = null;
            if (match.MatchData.Count != 0)
            {
                try
                {
                    pranges = PersistenceChunks.GetNewest(match.MatchData).ToArray();

                }
                catch (Exception e)
                {

                }
            }
            if (pranges != null)
            {

                match.CreateRanges(useRanges, pranges, FlushCallBack);
            }
            else
            {
                match.CreateRanges(useRanges);
                Console.WriteLine("warning: for stat chunk undeclared match data, not persistent");
            }
            //
            RetrievedModels.Add(match);
            return match;
        }
    }
}
