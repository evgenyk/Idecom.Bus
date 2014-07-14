using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.Sagas;
using Idecom.Bus.IoC.CastleWindsor;
using Idecom.Bus.Serializer.JsonNet;
using Idecom.Bus.Tests.InMemoryInfrastructure;
using Xunit;

namespace Idecom.Bus.Tests.Sagas
{
    public class TwoSagasCommunicationTests
    {
        [Fact]
        public void TwoSagasCanTalkToEachOtherWhileKeepingStateSeparateTest()
        {
            InMemorySagaPersister sagaPersister1 = null;
            InMemorySagaPersister sagaPersister2 = null;

            IBusInstance bus1 = Configure.With()
                .WindsorContainer()
                .InMemoryTransport()
                .InMemoryPubSub()
                .JsonNetSerializer()
                .ExposeConfiguration(x => { sagaPersister1 = x.Container.Resolve<InMemorySagaPersister>(); })
                .CreateBus("app1")
                .Start();
            bus1.Raise<IStartFirstSagaEvent>();
            Assert.True(Saga1.Started);

            IBusInstance bus2 = Configure.With()
                .WindsorContainer()
                .InMemoryTransport()
                .InMemoryPubSub()
                .JsonNetSerializer()
                .ExposeConfiguration(x => { sagaPersister2 = x.Container.Resolve<InMemorySagaPersister>(); })
                .CreateBus("app2")
                .Start();
        }


    }

    public class Saga1 : Saga<Saga1State>, IStartThisSagaWhenReceive<IStartFirstSagaEvent>
    {
        public static bool Started;
        public void Handle(IStartFirstSagaEvent command)
        {
            Started = true;
        }
    }

    public interface IStartFirstSagaEvent
    {
    }

    public class Saga1State : ISagaState
    {
    }

    public class Saga2: Saga<Saga2State>{}

    public class Saga2State : ISagaState
    {
    }
}