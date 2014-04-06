using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskUniversum;
using TaskUniversum.Statistics;

namespace TaskBroker.Statistics
{
    public delegate void FlushCB(StatRange range, Dictionary<string, object> match);
    public class StatHub
    {
        ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        public static int[] useRanges = new int[] { StatRange.seconds30, StatRange.min, StatRange.min30, StatRange.hour2 };
        //private const int aliveCount = 20160;// 1week for 30 sec interval, 2week's for minute, 420day's for 30 min interval...
        private const int aliveCount = 2880;// 1day for 30 sec interval

        private MongoDBPersistence PersistenceChunks;
        public StatHub()
        {
            string cc = System.Configuration.ConfigurationManager.AppSettings["statConnectionString"];
            string ccc = System.Configuration.ConfigurationManager.AppSettings["statConnectionDatabase"];
            if (string.IsNullOrWhiteSpace(cc) || string.IsNullOrWhiteSpace(ccc))
            {
                logger.Error("checkout app.config, set statistic mongodb connection parameters!");
                return;
            }
            PersistenceChunks = new MongoDBPersistence(cc, ccc);
        }
        public void Clear()
        {
            RetrievedModels.Clear();
        }
        private void OptimisePerformance(BrokerStat matchData)
        {
            try
            {
                OptimisePerformance(matchData.MatchData);
            }
            catch (Exception e)
            {
                logger.Error("can't ensure statistic collection index for: {0}:{1} '{2}'", matchData.Role, matchData.Name, e.Message);
            }
        }
        private void OptimisePerformance(Dictionary<string, object> matchData)
        {
            PersistenceChunks.EnsureIndex(matchData);
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
        }
        public List<StatMatchModel> RetrievedModels = new List<StatMatchModel>();

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
                logger.Exception(e, "FlushCallBack", "can't save statistic chunk");
            }
        }

        public BrokerStat InitialiseModel(BrokerStat instanceMatch)
        {
            OptimisePerformance(instanceMatch);
            return InitialiseModel<BrokerStat>(instanceMatch);
        }
        private T InitialiseModel<T>(T match)
            where T : StatMatchModel
        {
            MongoRange[] pranges = null;

            // restore from persistent component
            if (match.MatchData.Count != 0)
            {
                try
                {
                    pranges = PersistenceChunks.GetNewest(match.MatchData).ToArray();
                }
                catch (Exception e)
                {
                    logger.Exception(e, "current statistic state restore", "PersistenceChunks.GetNewest");
                }
            }
            if (pranges != null)
            {

                match.CreateRanges(useRanges, pranges, FlushCallBack);
            }
            else
            {
                match.CreateRanges(useRanges);
                logger.Warning("for stat chunk undeclared match data, not persistent");
            }
            //
            RetrievedModels.Add(match);
            return match;
        }
    }
}
