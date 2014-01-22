using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskQueue.Providers;

namespace SourceControl.buildServers
{
    public interface BuildServer
    {
        public string Name;
        public string Description;

        public TaskMessage GetParametersModel();
        public void SetParameters(TaskMessage parameters);

        public byte[] GetArtifacts();
    }
}
