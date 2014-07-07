using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskUniversum.Assembly;
using TaskUniversum.Common;
using TaskUniversum.Statistics;

namespace TaskUniversum
{
    public delegate void RestartApplication();
    public interface IBroker
    {
        void RegisterTempTask(Task.MetaTask mst, IBrokerModule module);
        RestartApplication resetBroker { get; }
        RestartApplication restartApp { get; }
        IEnumerable<KeyValuePair<string, IAssemblyStatus>> GetSourceStatuses();

        TaskQueue.RepresentedModel GetValidationModel(string MessageType, string ChannelName = null);
        Dictionary<string, string> GetCurrentChannelMTypeMap();

        bool PushMessage(TaskQueue.Providers.TaskMessage msg);

        void RegisterNewConfiguration(string id, string body);
        bool ValidateAndCommitConfigurations(string MainID, string ModulesID, string AssembliesID, out string errors, bool Reset = false, bool Restart = false);

        //string GetCurrentConfiguration(bool Main, bool Modules, bool Assemblys, bool Extra);
        IRepresentedConfiguration GetCurrentConfiguration(bool Main, bool Modules, bool Assemblys, bool Extra);

        ISourceManager GetSourceManager();
        StatisticContainer GetChannelsStatistic();
        // Module API's: logger, eventBus, ...
        ILogger APILogger();

    }
}
