namespace Idecom.Bus.Tests.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Implementations;
    using InMemoryInfrastructure;
    using IoC.CastleWindsor;
    using Serializer.JsonNet;
    using Xunit;

    public class OutgoingMessageValidationTests
    {
        [Fact(Skip = "Waiting for behaviors")]
        public void CanValidateWhileSendingAMessageTest()
        {
            var bus = Configure.With()
                               .WindsorContainer()
                               .InMemoryTransport()
                               .InMemoryPubSub()
                               .JsonNetSerializer()
                               .CreateBus("app1")
                               .Start();

            Assert.Throws<Exception>(() => bus.SendLocal(new Tests.ACommand()));
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
}