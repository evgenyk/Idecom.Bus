namespace Idecom.Bus.Tests.Sagas
{
    using System;
    using Implementations;
    using InMemoryInfrastructure;
    using IoC.CastleWindsor;
    using Serializer.JsonNet;
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

            var bus1 = Configure.With()
                                .WindsorContainer()
                                .InMemoryTransport()
                                .InMemoryPubSub()
                                .JsonNetSerializer()
                                .ExposeConfiguration(x => { subscriptionStorage = x.Container.Resolve<InMemorySubscriptionStorage>(); })
                                .DefineEventsAs(type => type.Namespace != null && type.Namespace.Equals("Idecom.Bus.Tests.Sagas.TwoSagas.Messages", StringComparison.InvariantCultureIgnoreCase))
                                .DefineHandlersAs(type => type.Namespace != null && type.Namespace.Equals("Idecom.Bus.Tests.Sagas.TwoSagas.FirstSaga", StringComparison.InvariantCultureIgnoreCase))
                                .CreateBus("app1")
                                .Start();

            var bus2 = Configure.With()
                                .WindsorContainer()
                                .ExposeConfiguration(x => x.Container.ConfigureInstance(subscriptionStorage))
                                .InMemoryTransport()
                                .InMemoryPubSub()
                                .JsonNetSerializer()
                                .DefineEventsAs(type => type.Namespace != null && type.Namespace.Equals("Idecom.Bus.Tests.Sagas.TwoSagas.Messages", StringComparison.InvariantCultureIgnoreCase))
                                .DefineHandlersAs(type => type.Namespace != null && type.Namespace.Equals("Idecom.Bus.Tests.Sagas.TwoSagas.SecondSaga", StringComparison.InvariantCultureIgnoreCase))
                                .CreateBus("app2")
                                .Start();

            bus1.Raise<IStartFirstSagaEvent>();

            Assert.True(Saga1.Started);
            Assert.True(Saga2.Started);
        }
    }
}