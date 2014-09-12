namespace Idecom.Bus.Tests.Sagas
{
    using System.Linq;
    using Implementations;
    using InMemoryInfrastructure;
    using Interfaces;
    using Interfaces.Addons.Sagas;
    using IoC.CastleWindsor;
    using Serializer.JsonNet;
    using Xunit;
    using Xunit.Should;

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
                               .ExposeConfiguration(x =>
                                                    {
                                                        inMemorySagaPersister = x.Container.Resolve<InMemorySagaPersister>();
                                                        x.Container.ConfigureInstance(new InMemoryBroker());
                                                    })
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