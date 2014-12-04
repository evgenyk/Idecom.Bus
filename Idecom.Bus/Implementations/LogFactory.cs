namespace Idecom.Bus.Implementations
{
    using System;
    using Interfaces;
    using Interfaces.Logging;

    public class LogFactory : ILogFactory
    {
        readonly Func<string, ILog> _logFactory;

        public LogFactory(Func<string, ILog> logFactory)
        {
            _logFactory = logFactory;
        }

        public ILog GetLogger(string name)
        {
            if (_logFactory == null) {
                throw new Exception("Log factory is not configured, please set LogFactory.Set(...)");
            }
            return _logFactory(name);
        }
    }
}