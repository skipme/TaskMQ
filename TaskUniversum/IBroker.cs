using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TaskUniversum
{
    public delegate void RestartApplication();
    public interface IBroker
    {
        void RegisterTempTask(Task.MetaTask mst, IBrokerModule module);
        RestartApplication resetBroker { get; }
        RestartApplication restartApp { get; }
    }
}
