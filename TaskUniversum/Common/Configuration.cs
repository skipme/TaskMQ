using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum.Common
{
    public interface IRepresentedConfiguration
    {
        string SerialiseJsonString();
    }
}
