﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskScheduler
{
    public delegate void PlanItemEntryPoint(ThreadContext ti, PlanItem pi);
}