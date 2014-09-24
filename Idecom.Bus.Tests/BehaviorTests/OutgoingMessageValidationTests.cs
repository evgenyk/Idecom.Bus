namespace Idecom.Bus.Tests.BehaviorTests
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Implementations;
    using Implementations.Behaviors;
    using InMemoryInfrastructure;
    using Interfaces.Behaviors;
    using IoC.CastleWindsor;
    using Serializer.JsonNet;
    using TestingInfrustructure;
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
                               .ExposeConfiguration(x => x.Container.ConfigureInstance(new InMemoryBroker()))
                               .InMemoryTransport()
                               .InMemoryPubSub()
                               .JsonNetSerializer()
                               .CreateTestBus("app1")
                               .Start();

            Assert.Throws<ValidationException>(() => bus.Send(new ACommand()));
            Assert.Throws<ValidationException>(() => bus.SendLocal(new ACommand()));
            Assert.Throws<ValidationException>(() => bus.Raise<ACommand>());
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