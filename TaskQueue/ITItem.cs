using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue
{
    public interface ITItem
    {
        List<string> EnumerateKeys();
        IEnumerable<ValueMapItem<string, TItemValue>> GetValues();

        bool Processed { get; set; }
        DateTime AddedTime { get; set; }
        DateTime? ProcessedTime { get; set; }
    }
}
