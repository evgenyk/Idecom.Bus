﻿namespace Idecom.Bus.Tests.Sagas
{
    using System;
    using System.Linq;
    using Castle.Core.Internal;
    using Implementations;
    using IoC.CastleWindsor;
    using Logging.Log4Net;
    using Serializer.JsonNet;
    using Testing.InMemoryInfrastructure;
    using Testing.TestingInfrustructure;
    using TwoSagas.FirstSaga;
    using TwoSagas.Messages;
    using TwoSagas.SecondSaga;
    using Xunit;

    public class TwoSagasCommunicationTests
    {
        [Fact]
        public void TwoSagasCanTalkToEachOtherWhileKeepingStateSeparateTest()
        {
            InMemorySubscriptionStorage subscriptionStorage = null;
            var inMemoryBroker = new InMemoryBroker();

            var bus1 = Configure.With()
                                .WindsorContainer()
                                .Log4Net()
                                .InMemoryTransport()
                                .InMemoryPubSub()
                                .JsonNetSerializer()
                                .ExposeConfiguration(x =>
                                                     {
                                                         subscriptionStorage = x.Container.Resolve<InMemorySubscriptionStorage>();
                                                         x.Container.ConfigureInstance(inMemoryBroker);
                                                     })
                                .DefineEventsAs(type => type.Namespace != null && type.Namespace.Equals("Idecom.Bus.Tests.Sagas.TwoSagas.Messages", StringComparison.InvariantCultureIgnoreCase))
                                .DefineHandlersAs(type => type.Namespace != null && type.Namespace.Equals("Idecom.Bus.Tests.Sagas.TwoSagas.FirstSaga", StringComparison.InvariantCultureIgnoreCase))
                                .CreateTestBus("app1")
                                .Start();

            var bus2 = Configure.With()
                                .WindsorContainer()
                                .Log4Net()
                                .ExposeConfiguration(x =>
                                                     {
                                                         x.Container.ConfigureInstance(inMemoryBroker);
                                                         x.Container.ConfigureInstance(subscriptionStorage);
                                                     })
                                .InMemoryTransport()
                                .InMemoryPubSub()
                                .JsonNetSerializer()
                                .DefineEventsAs(type => type.Namespace != null && type.Namespace.Equals("Idecom.Bus.Tests.Sagas.TwoSagas.Messages", StringComparison.InvariantCultureIgnoreCase))
                                .DefineHandlersAs(type => type.Namespace != null && type.Namespace.Equals("Idecom.Bus.Tests.Sagas.TwoSagas.SecondSaga", StringComparison.InvariantCultureIgnoreCase))
                                .CreateTestBus("app2")
                                .Start();


            //Enumerable.Range(1, 15).AsParallel().ForEach(i => bus1.Raise<IStartFirstSagaEvent>());
            
            bus1.Publish<IStartFirstSagaEvent>();

            Assert.True(Saga1.Started);
            Assert.True(Saga2.Started);
        }
    }
}