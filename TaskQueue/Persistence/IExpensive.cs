using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue.Persistence
{
    interface IExpensive
    {
        void SwapExpensive();
        void RestoreRequired();
    }
}
