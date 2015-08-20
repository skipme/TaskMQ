using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskUniversum
{
    public enum LogFrameEvent
    {
        unknown,
        Info,
        Warning,
        Error,
        Debug,
        Exception
    }
    public class LogTapeFrame
    {
        public LogTapeFrame()
        {
            EventType = LogFrameEvent.unknown;
        }
        public LogFrameEvent EventType;

        public DateTime Time;
        public string Message;
        public string EventName;
        public string Scope;

        public virtual string ToStringHeader()
        {
            return string.Format("{0} [{1}]    ", Time, EventName);
        }
        public virtual string ToStringMessage()
        {
            return string.Format("{0}: {1}", Scope, Message);
        }
        public override string ToString()
        {
            return string.Format("{0} [{1}]    {2}: {3}", Time, EventName, Scope, Message);
        }
    }
    public class LogTapeFrameException : LogTapeFrame
    {
        public LogTapeFrameException()
        {
            EventType = LogFrameEvent.Exception;
        }
        public string ExceptionMessage;
        public string ExceptionStackTrace;
        public string failedOperationDescription;

        public override string ToStringMessage()
        {
            return string.Format("{0}: {1}\r\n   MESSAGE: {2}\r\n CS: {3}", Scope, Message, ExceptionMessage, ExceptionStackTrace);
        }
        public override string ToString()
        {
            return string.Format("{0} [{1}]  {2}: {3}\r\n   EXCEPTION for: '{6}': {4}, {5}\r\n", Time, EventName, Scope, Message, ExceptionMessage, ExceptionStackTrace, failedOperationDescription);
        }
    }
    public interface ILogTape
    {
        void RecordFrame(LogTapeFrame frame);
    }
    public interface ILogger
    {
        void Write(string format, params object[] args);

        void Info(string format, params object[] args);
        void Warning(string format, params object[] args);
        void Error(string format, params object[] args);

        void Debug(string format, params object[] args);
        void Exception(Exception ex, string failedOperationDescription, string format, params object[] args);
    }
    public interface IScopeLogger
    {
        string scopeSource { get; }
    }
}
