using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TaskBroker.Logger
{
    class ConsoleEndpoint : LoggerEndpoint
    {
        public override void WriteFrame(TaskUniversum.LogTapeFrame frame)
        {
            switch (frame.EventType)
            {
                case TaskUniversum.LogFrameEvent.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case TaskUniversum.LogFrameEvent.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case TaskUniversum.LogFrameEvent.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    break;
                case TaskUniversum.LogFrameEvent.Exception:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            Console.WriteLine(frame);
            Console.ResetColor();
        }
    }
}
