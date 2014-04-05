using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TaskBroker.Logger
{
    public class ConsoleEndpoint : LoggerEndpoint
    {

        public override void WriteFrame(TaskUniversum.LogTapeFrame frame)
        {
            bool resetreq = false;
            switch (frame.EventType)
            {
                case TaskUniversum.LogFrameEvent.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    resetreq = true;
                    break;
                case TaskUniversum.LogFrameEvent.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    resetreq = true;
                    break;
                case TaskUniversum.LogFrameEvent.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    resetreq = true;
                    break;
                case TaskUniversum.LogFrameEvent.Exception:
                    Console.ForegroundColor = ConsoleColor.Red;
                    resetreq = true;
                    break;
            }
            Console.WriteLine(frame);
            if (resetreq)
                Console.ResetColor();
        }
    }
}
