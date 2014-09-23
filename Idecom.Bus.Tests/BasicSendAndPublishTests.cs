namespace Idecom.Bus.Tests
{
    using System;
    using Addressing;
    using Implementations;
    using Implementations.UnicastBus;
    using InMemoryInfrastructure;
    using Interfaces;
    using Interfaces.Behaviors;
    using IoC.CastleWindsor;
    using Serializer.JsonNet;
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
                               .CreateBus("app1")
                               .Start();

            bus.SendLocal(new ACommand());
            //_commandsHandled.ShouldBeGreaterThan(0);
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

            var bus1 = container.Resolve<IBus>();
            var bus2 = container.Resolve<TestBus>();

            bus.Raise<IEvent>(e => { });
            bus.MessagesReceived.ShouldBe(1);
        }
    }

    public class TestBus: Bus
    {
        public new TestBus Start()
        {
            base.Start();
            return this;
        }

        public object MessagesReceived { get; private set; }
    }

    public class IncomingTransportMessageTraceBehavior : IBehavior
    {
        readonly TestBus _testBus;

        public IncomingTransportMessageTraceBehavior(TestBus testBus)
        {
            _testBus = testBus;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {
            next();
        }
    }

    public static class BusExtensions
    {
        public static TestBus CreateTestBus(this Configure config, string queueName)
        {
            config.Container.ConfigureInstance(new Address(queueName));
            config.Container.Configure<TestBus>(ComponentLifecycle.Singleton);

            var bus = config.Container.Resolve<TestBus>();

            config.Container.ParentContainer.ConfigureInstance(bus);
            config.Container.Release(bus);

            bus.Chains.WrapWith<IncomingTransportMessageTraceBehavior>(ChainIntent.TransportMessageReceive);

            return bus;
        }
    }

    public class ACommand
    {
    }

    public interface IEvent
    {
    }
}