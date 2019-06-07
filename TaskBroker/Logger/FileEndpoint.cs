using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TaskBroker.Logger
{
    public class FileEndpoint : LoggerEndpoint, IDisposable
    {
        private StreamWriter fileStream;
        public FileEndpoint(string fileName, bool append = false)
        {
            FileStream fs = new FileStream(fileName, 
                append ? FileMode.OpenOrCreate : FileMode.Create, FileAccess.Write, 
                FileShare.Read);

            if (append)
            {
                fs.Seek(0, SeekOrigin.End);
            }

            fileStream = new StreamWriter(fs, Encoding.UTF8);
        }
        bool Disposed;
        ~FileEndpoint()
        {
            if (Disposed) return;
            Dispose(false);
        }
        public override void WriteFrame(TaskUniversum.LogTapeFrame frame)
        {
            if (Disposed) return;
            fileStream.WriteLine(frame);
            fileStream.Flush();
        }
        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.Disposed)
            {
                fileStream.Close();
                fileStream = null;

                Disposed = true;
            }
        }
    }
}
