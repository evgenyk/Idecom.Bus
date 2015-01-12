namespace Idecom.Bus.Tests.BehaviorTests
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Implementations;
    using Implementations.Behaviors;
    using Interfaces;
    using Interfaces.Behaviors;
    using IoC.CastleWindsor;
    using Logging.Log4Net;
    using Sagas.TwoSagas.FirstSaga;
    using Sagas.TwoSagas.Messages;
    using Sagas.TwoSagas.SecondSaga;
    using Serializer.JsonNet;
    using Testing.InMemoryInfrastructure;
    using Testing.TestingInfrustructure;
    using Xunit;

    public class OutgoingMessageValidationTests
    {
        [Fact]
        public void CanBuildBehaviorChainTest()
        {
            var chain = new BehaviorChain();
            chain.WrapWith<ExceptionBehavior>();
            chain.WrapWith<ExceptionBehavior>();
            Assert.Equal(2, chain.Count());
        }


        [Fact]
        public void CanValidateWhileSendingAMessageTest()
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

            Assert.Throws<ValidationException>(() => bus.Send(new ACommand()));
            Assert.Throws<ValidationException>(() => bus.SendLocal(new ACommand()));
            Assert.Throws<ValidationException>(() => bus.Publish<ACommand>());
        }

        [Fact]
        public void DelayedMessagesAreValidatedAsWellTest()
        {
            var inMemoryBroker = new InMemoryBroker(false);
            var bus1 = Configure.With()
                               .WindsorContainer()
                               .Log4Net()
                               .ExposeConfiguration(x =>
                                                    {
                                                        x.Container.ConfigureInstance(inMemoryBroker);
                                                    })
                               .InMemoryTransport()
                               .InMemoryPubSub()
                               .JsonNetSerializer()
                               .RouteMessagesFromNamespaceTo<ACommand>("app2")
                               .DefineHandlersAs(type => false)
                               .CreateTestBus("app1")
                               .Start();
            
            var bus2 = Configure.With()
                               .WindsorContainer()
                               .Log4Net()
                               .ExposeConfiguration(x => x.Container.ConfigureInstance(inMemoryBroker))
                               .InMemoryTransport()
                               .InMemoryPubSub()
                               .JsonNetSerializer()
                               .RouteMessagesFromNamespaceTo<Tests.IEvent>("app1")
                               .CreateTestBus("app2")
                               .Start();

            Assert.Throws<ValidationException>(() => bus1.Send(new ACommand(){InvalidProperty = "blah"}));
            
        }

        public class Bus1Replier : IHandle<ACommand>
        {
            public IBus Bus { get; set; }
            public void Handle(ACommand command)
            {
                Bus.Reply(new ACommand());
            }
        }

        public class ACommand
        {
            [Required]
            public string InvalidProperty { get; set; }
        }

        public interface IEvent
        {
        }
    }

    public class ExceptionBehavior : IBehavior
    {
        public void Execute(Action next, IChainExecutionContext context)
        {
            throw new Exception();
        }
    }
}