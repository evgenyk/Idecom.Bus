namespace Idecom.Bus.Tests
{
    using Implementations;
    using InMemoryInfrastructure;
    using Interfaces;
    using IoC.CastleWindsor;
    using Serializer.JsonNet;
    using Xunit;
    using Xunit.Should;

    public class BasicSendAndPublishTests : IHandle<ACommand>, IHandle<IEvent>
    {
        static volatile int _commandsHandled;
        static volatile int _eventsHandled;

        public void Handle(ACommand command)
        {
            _commandsHandled++;
        }

        public void Handle(IEvent command)
        {
            _eventsHandled++;
        }

        [Fact]
        public void SendingASimpleMessageShouldHandleAMessage()
        {
            var bus = Configure.With()
                               .WindsorContainer()
                               .InMemoryTransport()
                               .InMemoryPubSub()
                               .JsonNetSerializer()
                               .CreateBus("app1")
                               .Start();

            bus.SendLocal(new ACommand());
            _commandsHandled.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void RaisingASimpleeventShouldHandleThisEvent()
        {
            var bus = Configure.With()
                               .WindsorContainer()
                               .InMemoryTransport()
                               .InMemoryPubSub()
                               .JsonNetSerializer()
                               .CreateBus("app1")
                               .Start();

            bus.Raise<IEvent>(e => { });
            _eventsHandled.ShouldBeGreaterThan(0);
        }
    }

    public class ACommand
    {
    }

    public interface IEvent
    {
    }
}