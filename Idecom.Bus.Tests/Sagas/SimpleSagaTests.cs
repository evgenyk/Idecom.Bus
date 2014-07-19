using System.Linq;
using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.Sagas;
using Idecom.Bus.IoC.CastleWindsor;
using Idecom.Bus.Serializer.JsonNet;
using Idecom.Bus.Tests.InMemoryInfrastructure;
using Xunit;
using Xunit.Should;

namespace Idecom.Bus.Tests.Sagas
{
    public class SimpleSagaTests
    {
        [Fact]
        public void CanStartAndFinishSagaTest()
        {
            InMemorySagaPersister inMemorySagaPersister = null;
            var bus = Configure.With()
                               .WindsorContainer()
                               .InMemoryTransport()
                               .InMemoryPubSub()
                               .JsonNetSerializer()
                               .ExposeConfiguration(x => { inMemorySagaPersister = x.Container.Resolve<InMemorySagaPersister>(); })
                               .DefineEventsAs(type => type == typeof (IStartSimpleSagaEvent) || type == typeof (ICompleteSimpleSagaEvent))
                               .CreateBus("app1")
                               .Start();

            bus.Raise<IStartSimpleSagaEvent>();
            inMemorySagaPersister.SagaStorage.Count().ShouldBe(0);
        }
    }


    public class SimpleSaga : Saga<SimpleSagaData>,
                              IStartThisSagaWhenReceive<IStartSimpleSagaEvent>,
                              IHandle<ICompleteSimpleSagaEvent>
    {
        public void Handle(ICompleteSimpleSagaEvent command)
        {
            CloseSaga();
        }

        public void Handle(IStartSimpleSagaEvent command)
        {
            Bus.Raise<ICompleteSimpleSagaEvent>();
        }
    }

    public interface IStartSimpleSagaEvent
    {
    }

    public interface ICompleteSimpleSagaEvent
    {
    }

    public class SimpleSagaData : ISagaState
    {
    }
}