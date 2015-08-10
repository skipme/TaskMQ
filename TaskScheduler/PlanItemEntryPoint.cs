using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskScheduler
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="thread"></param>
    /// <param name="job"></param>
    /// <returns>suspend or not 1/0</returns>
    public delegate int PlanItemEntryPoint(ThreadContext thread, PlanItem job);
}
