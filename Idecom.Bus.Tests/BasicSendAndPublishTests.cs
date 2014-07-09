using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.IoC.CastleWindsor;
using Idecom.Bus.Serializer.JsonNet;
using Idecom.Bus.Tests.InMemoryInfrastructure;
using Xunit;
using Xunit.Should;

namespace Idecom.Bus.Tests
{
    public class BasicSendAndPublishTests : IHandle<ACommand>, IHandle<IEvent>
    {
        private static volatile int _commandsHandled;
        private static volatile int _eventsHandled;

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