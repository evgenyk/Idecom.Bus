namespace Idecom.Bus.IoC.CastleWindsor
{
    using System;
    using Castle.Windsor;
    using Implementations;

    public static class Bootstrap
    {
        public static Configure WindsorContainer(this ConfigureContainer configure, IWindsorContainer container = null, Func<IDisposable> beginUnitOfWorkFunc = null)
        {
            configure.Container = new WindsorContainerAdapter(container).CreateChildContainer();
            configure.Container.ConfigureInstance(configure.Container);
            configure.Container.ConfigureUnitOfWork(beginUnitOfWorkFunc);
            return configure.RootConfiguration;
        }
    }
}