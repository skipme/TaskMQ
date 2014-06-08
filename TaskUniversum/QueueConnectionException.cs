using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum
{
    [Serializable]
    public class QueueConnectionException : Exception
    {
        public QueueConnectionException() { }
        public QueueConnectionException(string message) : base(message) { }
        public QueueConnectionException(string message, Exception inner) : base(message, inner) { }
        protected QueueConnectionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
