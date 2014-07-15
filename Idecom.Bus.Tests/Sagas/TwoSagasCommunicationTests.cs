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
        [Fact(Skip = "Can't talk to each other yet. In fact they hate each other.")]
        public void TwoSagasCanTalkToEachOtherWhileKeepingStateSeparateTest()
        {
            InMemorySagaPersister sagaPersister1 = null;
            InMemorySagaPersister sagaPersister2 = null;

            var bus1 = Configure.With()
                .WindsorContainer()
                .InMemoryTransport()
                .InMemoryPubSub()
                .JsonNetSerializer()
                .ExposeConfiguration(x => { sagaPersister1 = x.Container.Resolve<InMemorySagaPersister>(); })
                .DefineEventsAs(type => type.Namespace != null && type.Namespace.Equals("Idecom.Bus.Tests.Sagas.TwoSagas.Messages", StringComparison.InvariantCultureIgnoreCase))
                .CreateBus("app1")
                .Start();

            var bus2 = Configure.With()
                .WindsorContainer()
                .InMemoryTransport()
                .InMemoryPubSub()
                .JsonNetSerializer()
                .ExposeConfiguration(x => { sagaPersister2 = x.Container.Resolve<InMemorySagaPersister>(); })
                .DefineEventsAs(type => type.Namespace != null && type.Namespace.Equals("Idecom.Bus.Tests.Sagas.TwoSagas.Messages", StringComparison.InvariantCultureIgnoreCase))
                .CreateBus("app2")
                .Start();

            bus1.Raise<IStartFirstSagaEvent>();

            Assert.True(Saga1.Started);
            Assert.True(Saga2.Started);
        }
    }
}