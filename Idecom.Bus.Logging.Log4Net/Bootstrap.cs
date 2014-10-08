namespace Idecom.Bus.Logging.Log4Net
{
    using Implementations;
    using Interfaces;

    public static class Bootstrap
    {
        public static Configure Log4Net(this ConfigureLogging configure)
        {
            configure.Container.Configure<Log4NetLogger>(ComponentLifecycle.Transient);
//            configure.Container = new WindsorContainerAdapter(container).CreateChildContainer();
//            configure.Container.ConfigureInstance(configure.Container);
//            configure.Container.ConfigureUnitOfWork(beginUnitOfWorkFunc);
            return configure.RootConfiguration;
        }
    }
}