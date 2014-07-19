using System;
using Idecom.Bus.Implementations;
using Idecom.Bus.IoC.CastleWindsor;
using Idecom.Bus.Serializer.JsonNet;
using Idecom.Bus.Tests.InMemoryInfrastructure;
using Idecom.Bus.Tests.Sagas.TwoSagas.FirstSaga;
using Idecom.Bus.Tests.Sagas.TwoSagas.Messages;
using Idecom.Bus.Tests.Sagas.TwoSagas.SecondSaga;
using Xunit;

namespace Idecom.Bus.Tests.Sagas
{
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