namespace Idecom.Bus.Interfaces.Logging
{
    using System;

    public interface ILog
    {
        void Debug(string message);
        void Debug(string message, Exception exception);
        void DebugFormat(string format, params object[] args);

        void Info(string message);
        void Info(string message, Exception exception);
        void InfoFormat(string format, params object[] args);

        void Error(string message);
        void Error(string message, Exception exception);
        void ErrorFormat(string format, params object[] args);
    }



}