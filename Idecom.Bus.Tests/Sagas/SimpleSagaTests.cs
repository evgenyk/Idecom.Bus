namespace Idecom.Bus.Tests.Sagas
{
    using System.Linq;
    using Implementations;
    using Interfaces;
    using Interfaces.Addons.Sagas;
    using IoC.CastleWindsor;
    using Logging.Log4Net;
    using Serializer.JsonNet;
    using Testing.InMemoryInfrastructure;
    using Testing.TestingInfrustructure;
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
                                .Log4Net()
                               .InMemoryTransport()
                               .InMemoryPubSub()
                               .JsonNetSerializer()
                               .ExposeConfiguration(x =>
                                                    {
                                                        inMemorySagaPersister = x.Container.Resolve<InMemorySagaPersister>();
                                                        x.Container.ConfigureInstance(new InMemoryBroker());
                                                    })
                               .DefineEventsAs(type => type == typeof (IStartSimpleSagaEvent) || type == typeof (ICompleteSimpleSagaEvent))
                               .CreateTestBus("app1")
                               .Start();

            bus.Raise<IStartSimpleSagaEvent>();
            inMemorySagaPersister.SagaStorage.Count().ShouldBe(0);
            bus.Stop();
        }

        [Fact]
        public void StartTenSagasCloseTenSagas()
        {
            InMemorySagaPersister inMemorySagaPersister = null;
            var bus = Configure.With()
                               .WindsorContainer()
                               .Log4Net()
                               .InMemoryTransport()
                               .InMemoryPubSub()
                               .JsonNetSerializer()
                               .ExposeConfiguration(x =>
                               {
                                   inMemorySagaPersister = x.Container.Resolve<InMemorySagaPersister>();
                                   x.Container.ConfigureInstance(new InMemoryBroker());
                               })
                               .DefineEventsAs(type => type == typeof(IStartSimpleSagaEvent) || type == typeof(ICompleteSimpleSagaEvent))
                               .CreateTestBus("app1")
                               .Start();

            for (int i = 0; i < 10; i++) {
                bus.Raise<IStartSimpleSagaEvent>();
            }
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