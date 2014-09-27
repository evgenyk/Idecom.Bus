﻿namespace Idecom.Bus.Tests.TestingInfrustructure
{
    using Addressing;
    using Implementations;
    using Implementations.UnicastBus;
    using Interfaces;

    public static class BusExtensions
    {
        public static TestBus CreateTestBus(this Configure config, string queueName)
        {
            config.Container.ConfigureInstance(new Address(queueName));
            config.Container.Configure<TestBus>(ComponentLifecycle.Singleton);

            var bus = config.Container.Resolve<TestBus>();

            config.Container.ParentContainer.ConfigureInstance(bus);
            config.Container.Release(bus);

            bus.Chains.WrapWith<IncomingTransportMessageTraceBehavior>(ChainIntent.TransportMessageReceive);

            return bus;
        }
    }
}