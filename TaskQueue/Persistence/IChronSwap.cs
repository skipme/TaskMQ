using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace TaskQueue.Persistence
{
    public interface IChronSwap
    {
        void Initialize(int rangeBlockSize);
        void Swap(int index, ICollection<TItemModel> swapRange);
        ICollection<TItemModel> Restore(int index);
        int MaxBlockIndex { get; }
    }
}
