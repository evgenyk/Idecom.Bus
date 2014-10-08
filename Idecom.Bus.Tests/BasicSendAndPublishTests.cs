namespace Idecom.Bus.Tests
{
    using System;
    using System.Collections;
    using System.Linq;
    using Implementations;
    using Interfaces;
    using IoC.CastleWindsor;
    using Logging.Log4Net;
    using Serializer.JsonNet;
    using Testing.InMemoryInfrastructure;
    using Testing.TestingInfrustructure;
    using Transport;
    using Xunit;
    using Xunit.Should;

    public class BasicSendAndPublishTests : IHandle<ACommand>, IHandle<IEvent>
    {

        public void Handle(ACommand command)
        {
        }

        public void Handle(IEvent command)
        {
        }

        [Fact]
        public void SendingASimpleMessageShouldHandleAMessage()
        {
            var bus = Configure.With()
                               .WindsorContainer()
                                .Log4Net()
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
            var bus = Configure.With()
                               .WindsorContainer()
                                .Log4Net()
                               .ExposeConfiguration(x => x.Container.ConfigureInstance(new InMemoryBroker()))
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