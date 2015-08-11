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
            ConsoleColor clrl = Console.ForegroundColor;
            switch (frame.EventType)
            {
                case TaskUniversum.LogFrameEvent.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    resetreq = true;
                    break;
                case TaskUniversum.LogFrameEvent.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    resetreq = true;
                    break;
                case TaskUniversum.LogFrameEvent.Debug:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    resetreq = true;
                    break;
                case TaskUniversum.LogFrameEvent.Exception:
                    Console.ForegroundColor = ConsoleColor.Red;
                    resetreq = true;
                    break;
            }
            Console.WriteLine(frame.ToString());
            if (resetreq)
                Console.ForegroundColor = clrl;
            //if (resetreq)
                //Console.ResetColor();
        }
    }
}
