using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Logger
{
    public abstract class LoggerEndpoint
    {
        public abstract void WriteFrame(TaskUniversum.LogTapeFrame frame);
    }
}
