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
                               .InMemoryTransport()
                               .InMemoryPubSub()
                               .JsonNetSerializer()
                               .CreateBus("app1")
                               .Start();

            Assert.Throws<Exception>(() => bus.Send(new Tests.ACommand()));
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
        public void Execute(Action next)
        {
            throw new Exception();
        }
    }
}