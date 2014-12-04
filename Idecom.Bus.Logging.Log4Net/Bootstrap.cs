namespace Idecom.Bus.Logging.Log4Net
{
    using Implementations;
    using Interfaces;

    public static class Bootstrap
    {
        public static Configure Log4Net(this ConfigureLogging configure)
        {
            configure.RootConfiguration.SetLogger((name) => new Log4NetLogger(name));
            return configure.RootConfiguration;
        }
    }
}