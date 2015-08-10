using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskQueue.Providers
{
    public class QueueOverflowException : Exception
    {
        public Exception baseException;

        public QueueOverflowException(Exception baseException)
        {
            this.baseException = baseException;
        }
    }
}
