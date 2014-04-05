using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Logger
{
    abstract class LoggerEndpoint
    {
        public abstract void WriteFrame(TaskUniversum.LogTapeFrame frame);
    }
}
