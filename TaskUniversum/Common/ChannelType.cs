using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum.Common
{
    public enum ChannelType
    {
        // if we have only one channel for messageType
        primary = 0,
        // classificator take care of it - compare every peek message in every cluster channel, dequeue at zero in suitable channel
        // this is experimental strategy vertical scaling with task copying threads increasing etc...
        cluster = 10,
        // classificator dequeue/enqueue only one channel per message operation
        cluster_disordered,

        fallback = 100  // push and pop every channel (hot reserve), store every unpushed and unupdated message in persistent journal
    }
}
