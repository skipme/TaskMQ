using System;
using System.Collections.Generic;

namespace TaskBroker.Logger
{
    public class ConsoleEndpoint : LoggerEndpoint
    {
        static readonly Dictionary<TaskUniversum.LogFrameEvent, ConsoleColor> cmap = new Dictionary<TaskUniversum.LogFrameEvent, ConsoleColor>
        {
             {TaskUniversum.LogFrameEvent.Warning, ConsoleColor.Yellow},
             {TaskUniversum.LogFrameEvent.Error, ConsoleColor.DarkRed},
             {TaskUniversum.LogFrameEvent.Debug, ConsoleColor.DarkYellow},
             {TaskUniversum.LogFrameEvent.Exception, ConsoleColor.Red}
        };
        public override void WriteFrame(TaskUniversum.LogTapeFrame frame)
        {
            ConsoleColor clrl = Console.ForegroundColor;
            ConsoleColor clrlset = 0;

            if (cmap.TryGetValue(frame.EventType, out clrlset))
            {
                Console.ForegroundColor = clrlset;
                Console.Write(frame.ToStringHeader());
                Console.ResetColor();
                Console.Write("    ");
                Console.WriteLine(frame.ToStringMessage());
            }
            else
            {
                Console.WriteLine(frame.ToString());
            }

        }
    }
}
