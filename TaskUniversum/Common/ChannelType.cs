using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum.Common
{
    public enum ChannelType
    {
        primary = 0,    // if we have only one channel for messageType
        cluster = 10,   // classificator take care of it - compare every peek message in every cluster channel, dequeue at zero in suitable channel
        fallback = 100  // push and pop every channel (hot reserve), store every unpushed and unupdated message in persistent journal
    }
}
