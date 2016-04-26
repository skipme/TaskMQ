using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskScheduler
{
    /// <summary>
    /// return 1 if executed logic don't need maximum priority, 0 if it need to maximum priority in queue
    /// this is actually for default plan strategy
    /// </summary>
    /// <param name="thread"></param>
    /// <param name="job"></param>
    /// <returns>suspend current thread or not 1/0</returns>
    public delegate int PlanItemEntryPoint(ThreadContext thread, PlanItem job);
}
