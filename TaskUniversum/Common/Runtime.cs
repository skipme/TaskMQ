using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum.Common
{
    public class Runtime
    {
        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}
