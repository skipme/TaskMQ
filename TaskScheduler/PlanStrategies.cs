using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskScheduler
{
    public enum PlanStrategies : byte
    {
        dynamic_sort_jobs = 0xAE, // DEFAULT. THREADS TAKE JOB FROM SORTED LIST. SORT IF LIST EMPTY
        tape_jobs = 0xCC,         /*
                                   * TAPE WITH MARKERS {start, delay} posted to thread until jobs not started at time(start marker)
                                   * after that tape is forked with this jobs and without jobs started at time
                                   * after job is done it records time required and try to predict time required for execution, for data{start, delay, time_rqe} we can produce more efficient tapes
                                   */ 
    }
}
