using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue.Distal
{
    public abstract class ChronObject : 
        TaskQueue.Providers.TItemModel,
        IComparable
    {
        //public TC IDkey;
        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
