namespace Idecom.Bus.Tests.BehaviorTests
{
    using System;
    using Addressing;
    using Implementations.UnicastBus;
    using Interfaces.Behaviors;
    using Transport;
    using Xunit;

    public class ChainExecutionContextTests
    {
        public static ChainExecutionContext RootContext;

        [Fact]
        public void OutgoingMessageAreReadFromTheRootContextTest()
        {
            var outgoingMessage = new object();

            var rootContext = AmbientChainContext.Current;
            using (var inner = rootContext.Push(context =>
                                    {
                                        context.OutgoingMessage = outgoingMessage;
                                    }))
            {
                Assert.Equal(inner.OutgoingMessage, outgoingMessage);

                using (var innerInner = inner.Push(context => {}))
                {
                    var expected = innerInner.OutgoingMessage;
                    Assert.Equal(expected, outgoingMessage);
                }
            }
            

        } 
        
        [Fact]
        public void IncommingMessageContextIsAlwaysAccessibleTest()
        {
            var incomingMessage = new object();

            RootContext = AmbientChainContext.Current;
            using (var inner = RootContext.Push(context =>
                                                {
                                                    var incomingTransportMessage = new TransportMessage(incomingMessage, new Address("src"), new Address("target"), MessageIntent.Publish, typeof(object));
                                                    var incomingMessageContext = new MessageContext(incomingTransportMessage,0, 0 );
                                                    context.IncomingMessageContext = incomingMessageContext;
                                                }))
            {
                Assert.NotNull(inner.IncomingMessageContext);

                using (var innerInner = inner.Push(context => {}))
                {
                    Assert.NotNull(innerInner.IncomingMessageContext);

                    var handlerTest = new HandlerContextTest();
                    handlerTest.GetType().GetMethod("Handle").Invoke(handlerTest, new[] { new object() });

                }
            }
            

        }

        class HandlerContextTest
        {
            public void Handle(object message)
            {
                Assert.NotNull(AmbientChainContext.Current.IncomingMessageContext);
            }
        }
    }
}