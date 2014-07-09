using System.Linq;
using Castle.Windsor;
using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.Sagas;
using Idecom.Bus.IoC.CastleWindsor;
using Idecom.Bus.Serializer.JsonNet;
using Idecom.Bus.Tests.InMemoryInfrastructure;
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
            sagaPersister.SagaStorage.Count().ShouldBe(0);
        } 
    }


    public class SimpleSaga: Saga<SimpleSagaData>,
        IStartThisSagaWhenReceive<IStartSimpleSagaEvent>,
        IHandle<ICompleteSimpleSagaEvent>
    {
        public void Handle(IStartSimpleSagaEvent command)
        {
            Bus.Raise<ICompleteSimpleSagaEvent>();
        }

        public void Handle(ICompleteSimpleSagaEvent command)
        {
            CloseSaga();
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