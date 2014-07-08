using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.IoC.CastleWindsor;
using Idecom.Bus.Serializer.JsonNet;
using Idecom.Bus.Tests.InMemoryInfrustructure;
using Xunit;
using Xunit.Should;

namespace Idecom.Bus.Tests
{
    public class SendTests : IHandle<ACommand>
    {
        private volatile static int _handlerCalledTimes;

        public void Handle(ACommand command)
        {
            _handlerCalledTimes++;
        }

        [Fact]
        public void SendingASimpleMessageShouldHandleAMessage()
        {
            var bus = Configure.With()
                .WindsorContainer()
                .InMemoryTransport()
                .JsonNetSerializer()
                .CreateBus("app1")
                .Start();

            bus.SendLocal(new ACommand());
            _handlerCalledTimes.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void RaisingASimpleeventShouldHandleThisEvent()
        {
            var bus = Configure.With()
                .WindsorContainer()
                .InMemoryTransport()
                .JsonNetSerializer()
                .CreateBus("app1")
                .Start();

            bus.Raise<IEvent>(e => { });
            _handlerCalledTimes.ShouldBeGreaterThan(0);
        }
    }

    public class ACommand
    {
    }

    public interface IEvent
    {
    }
}