namespace Idecom.Bus.Tests.Sagas
{
    using System;
    using Implementations;
    using IoC.CastleWindsor;
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

            bus1.Raise<IStartFirstSagaEvent>();

            Assert.True(Saga1.Started);
            Assert.True(Saga2.Started);
        }
    }
}