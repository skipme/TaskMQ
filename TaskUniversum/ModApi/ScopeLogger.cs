using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TaskUniversum.ModApi
{
    public class ScopeLogger : ILogger, IScopeLogger
    {
        private ILogTape baseLogger;

        public static ScopeLogger GetClassLogger(ILogTape baseLogger = null, int framesToSkip = 1)
        {
            // extracted from: https://github.com/NLog
            string loggerName;
            Type declaringType;

            do
            {
                StackFrame frame = new StackFrame(framesToSkip, false);

                var method = frame.GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    loggerName = method.Name;
                    break;
                }

                framesToSkip++;
                loggerName = declaringType.FullName;
            } while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return new ScopeLogger(loggerName, baseLogger);
        }

        private LogTapeFrame formFrame(string eventName, string message)
        {
            return new LogTapeFrame
            {
                EventName = eventName,
                Message = message,
                Scope = scopeSource,
                Time = DateTime.UtcNow
            };
        }
        private LogTapeFrame formFrameException(Exception ex, string failedOperationDescription, string message)
        {
            return new LogTapeFrameException
            {
                EventName = "Exception",

                Message = message,
                Scope = scopeSource,
                Time = DateTime.UtcNow,

                ExceptionMessage = ex.Message,
                ExceptionStackTrace = ex.StackTrace,
                failedOperationDescription = failedOperationDescription
            };
        }
        private void pushFrame(string eventName, string message)
        {
            pushFrame(formFrame(eventName, message));
        }
        private void pushFrame(LogTapeFrame frame)
        {
            if (baseLogger != null)
                baseLogger.RecordFrame(frame);
            else
                Console.WriteLine(frame);
        }
        public ScopeLogger(string scopeName, ILogTape baseLogger)
        {
            scopeSource = scopeName;
            this.baseLogger = baseLogger;
        }

        public void Write(string format, params object[] args)
        {
            pushFrame(string.Empty, string.Format(format, args));
        }

        public void Info(string format, params object[] args)
        {
            pushFrame("Info", string.Format(format, args));
        }

        public void Warning(string format, params object[] args)
        {
            pushFrame("Warning", string.Format(format, args));
        }

        public void Error(string format, params object[] args)
        {
            pushFrame("Error", string.Format(format, args));
        }

        public void Debug(string format, params object[] args)
        {
            pushFrame("Debug", string.Format(format, args));
        }

        public void Exception(Exception ex, string failedOperationDescription, string format, params object[] args)
        {
            pushFrame(formFrameException(ex, failedOperationDescription, string.Format(format, args)));
        }

        public string scopeSource
        {
            get;
            private set;
        }
    }
}
