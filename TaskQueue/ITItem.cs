using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue
{
    public interface ITItem
    {
        Dictionary<string, object> GetHolder();
    }
}
