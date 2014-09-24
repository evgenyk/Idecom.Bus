namespace Idecom.Bus.Tests
{
    using System.Collections;
    using System.Linq;
    using Implementations;
    using InMemoryInfrastructure;
    using Interfaces;
    using IoC.CastleWindsor;
    using Serializer.JsonNet;
    using TestingInfrustructure;
    using Transport;
    using Xunit;
    using Xunit.Should;

    public class BasicSendAndPublishTests : IHandle<ACommand>, IHandle<IEvent>
    {
//        static volatile int _commandsHandled;
//        static volatile int _eventsHandled;

        public void Handle(ACommand command)
        {
            //_commandsHandled++;
        }

        public void Handle(IEvent command)
        {
            //_eventsHandled++;
        }

        [Fact]
        public void SendingASimpleMessageShouldHandleAMessage()
        {
            var bus = Configure.With()
                               .WindsorContainer()
                               .ExposeConfiguration(x => x.Container.ConfigureInstance(new InMemoryBroker()))
                               .InMemoryTransport()
                               .InMemoryPubSub()
                               .JsonNetSerializer()
                               .CreateTestBus("app1")
                               .Start();

            bus.SendLocal(new ACommand());
            bus.MessagesReceived.OfType<ACommand>().Count().ShouldBe(1);
        }

        [Fact]
        public void RaisingASimpleeventShouldHandleThisEvent()
        {
            IContainer container = null;
            var bus = Configure.With()
                               .WindsorContainer()
                               .ExposeConfiguration(x =>
                                                    {
                                                        container = x.Container;
                                                        x.Container.ConfigureInstance(new InMemoryBroker());
                                                    })
                               .InMemoryTransport()
                               .InMemoryPubSub()
                               .JsonNetSerializer()
                               .CreateTestBus("app1")
                               .Start();

            bus.Raise<IEvent>(e => { });
            bus.MessagesReceived.OfType<IEvent>().Count().ShouldBe(1);
        }
    }

    public class ACommand
    {
    }

    public interface IEvent
    {
    }
}