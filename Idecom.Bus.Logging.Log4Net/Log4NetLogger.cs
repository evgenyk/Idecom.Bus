namespace Idecom.Bus.Logging.Log4Net
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Interfaces.Logging;
    using log4net;
    using ILog = Interfaces.Logging.ILog;

    public class Log4NetLogger: ILog
    {
        log4net.ILog _logger;

        log4net.ILog Logger
        {
            get
            {
                var declaringType = new StackFrame(2, true).GetMethod().DeclaringType;
                var loggerNameAttribute = declaringType.GetCustomAttribute<LoggerNameAttribute>();
                return _logger ?? (_logger = LogManager.GetLogger(loggerNameAttribute == null ? (declaringType == null ? "UnspecifiedLogger" : declaringType.Name) : loggerNameAttribute.Name));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Debug(string message)
        {
            Logger.Debug(message);
        }

        public void Debug(string message, Exception exception)
        {
        }

        public void DebugFormat(string format, params object[] args)
        {
        }

        public void Info(string message)
        {
        }

        public void Info(string message, Exception exception)
        {
        }

        public void InfoFormat(string format, params object[] args)
        {
        }

        public void Error(string message)
        {
        }

        public void Error(string message, Exception exception)
        {
        }

        public void ErrorFormat(string format, params object[] args)
        {
        }
    }
}