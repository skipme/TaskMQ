using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum
{
    public interface IBrokerModule
    {
        string UniqueName { get; }
        ExecutionType Role { get; }
        IMod MI { get; }
    }
}
