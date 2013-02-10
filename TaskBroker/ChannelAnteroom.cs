using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker
{
    public class ChannelAnteroom
    {
        public ChannelAnteroom()
        {

        }
        public TaskQueue.ITItem[] Tuple { get; set; }
        public int TupleCursor { get; set; }

        public TaskQueue.ITQueue Queue { get; set; }

        public TaskQueue.ITItem Next()
        {
            throw new NotImplementedException();
            // Tuple[TupleCursor++]
        }
    }
}
