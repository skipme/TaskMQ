using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TaskBroker.Logger
{
    public class FileEndpoint : LoggerEndpoint
    {
        StreamWriter fileStream;
        public FileEndpoint(string fileName, bool append = false)
        {
            FileStream fs = new FileStream(fileName, 
                append ? FileMode.OpenOrCreate : FileMode.Create, FileAccess.Write, 
                FileShare.ReadWrite);
            if (append)
                fs.Seek(0, SeekOrigin.End);

            fileStream = new StreamWriter(fs, Encoding.UTF8);
        }
        ~FileEndpoint()
        {
            fileStream.Close();
        }
        public override void WriteFrame(TaskUniversum.LogTapeFrame frame)
        {
            fileStream.WriteLine(frame);
            fileStream.Flush();
        }
    }
}
