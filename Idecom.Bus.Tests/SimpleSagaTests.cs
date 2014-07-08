using System.Linq;
using Castle.Windsor;
using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.Sagas;
using Idecom.Bus.IoC.CastleWindsor;
using Idecom.Bus.Serializer.JsonNet;
using Idecom.Bus.Tests.InMemoryInfrustructure;
using Xunit;
using Xunit.Should;

namespace Idecom.Bus.Tests
{
    public class SimpleSagaTests
    {
        [Fact]
        public void CanStartAndFinishSagaTest()
        {
            InMemorySagaPersister sagaPersister = null;

            var bus = Configure.With()
                .WindsorContainer()
                .InMemoryTransport()
                .InMemoryPubSub()
                .JsonNetSerializer()
                .ExposeConfiguration(x =>
                {
                    sagaPersister = x.Container.Resolve<InMemorySagaPersister>();
                })
                .CreateBus("app1")
                .Start();
            bus.Raise<IStartSimpleSagaEvent>();
            sagaPersister.SagaStorage.Count().ShouldBeGreaterThan(0);
            bus.Raise<IStopSimpleSagaEvent>();
            sagaPersister.SagaStorage.Count().ShouldBeGreaterThan(0);
        } 
    }


    public class SimpleSaga: Saga<SimpleSagaData>,
        IStartThisSagaWhenReceive<IStartSimpleSagaEvent>,
        IHandle<IStopSimpleSagaEvent>
    {
        public void Handle(IStartSimpleSagaEvent command)
        {
        }

        public void Handle(IStopSimpleSagaEvent command)
        {
            CloseSaga();
        }
    }

    public interface IStartSimpleSagaEvent
    {
    }

    public interface IStopSimpleSagaEvent
    {
    }

    public class SimpleSagaData : ISagaState
    {
    }
}