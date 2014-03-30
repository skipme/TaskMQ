using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum
{
    public interface IBroker
    {
        void RegisterTempTask(TaskUniversum.Task mst, IBrokerModule module);
    }
}
