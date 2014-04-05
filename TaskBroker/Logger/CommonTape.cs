using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Logger
{
    public class CommonTape : TaskUniversum.ILogTape
    {
        List<LoggerEndpoint> Endpoints;
        public CommonTape(LoggerEndpoint[] endpoints)
        {
            Endpoints = new List<LoggerEndpoint>(endpoints);
        }

        public void RecordFrame(TaskUniversum.LogTapeFrame frame)
        {
            foreach (var ep in Endpoints)
            {
                ep.WriteFrame(frame);
            }
        }
    }
}
