namespace Idecom.Bus.Logging.Log4Net
{
    using System;
    using log4net;
    using ILog = Interfaces.Logging.ILog;

    public class Log4NetLogger : ILog
    {
        readonly log4net.ILog _logger;

        public Log4NetLogger(string name)
        {
            _logger = LogManager.GetLogger(name);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Debug(string message, Exception exception)
        {
            _logger.Debug(message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            _logger.DebugFormat(format, args);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Info(string message, Exception exception)
        {
            _logger.Info(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            _logger.InfoFormat(format, args);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(string message, Exception exception)
        {
            _logger.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            _logger.ErrorFormat(format, args);
        }
    }
}