using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TaskBroker.Logger
{
    class FileEndpoint : LoggerEndpoint
    {
        StreamWriter fileStream;
        public FileEndpoint(string fileName)
        {
            fileStream = new StreamWriter(fileName, false, Encoding.UTF8);
        }
        ~FileEndpoint()
        {
            fileStream.Close();
        }
        public override void WriteFrame(TaskUniversum.LogTapeFrame frame)
        {
            fileStream.WriteLine(frame);
        }
    }
}
