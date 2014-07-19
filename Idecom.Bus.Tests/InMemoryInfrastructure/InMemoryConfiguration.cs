﻿namespace Idecom.Bus.Tests.InMemoryInfrastructure
{
    using Implementations;
    using Interfaces;

    public static class InMemoryConfiguration
    {
        public static Configure InMemoryTransport(this Configure configure, int workersCount = 1, int retries = 1)
        {
            configure.Container.Configure<InMemoryTransport>(ComponentLifecycle.Singleton);
            configure.Container.ConfigureProperty<InMemoryTransport>(x => x.WorkersCount, workersCount);
            configure.Container.ConfigureProperty<InMemoryTransport>(x => x.Retries, retries);

            return configure;
        }

        public static Configure InMemoryPubSub(this Configure configure, int workersCount = 1, int retries = 1)
        {
            configure.Container.Configure<InMemorySubscriptionStorage>(ComponentLifecycle.Singleton);
            configure.Container.Configure<InMemorySagaPersister>(ComponentLifecycle.Singleton);

            return configure;
        }
    }
}