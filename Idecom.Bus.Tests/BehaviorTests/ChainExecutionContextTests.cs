namespace Idecom.Bus.Tests.BehaviorTests
{
    using Addressing;
    using Implementations.UnicastBus;
    using Interfaces.Behaviors;
    using Transport;
    using Xunit;

    public class ChainExecutionContextTests
    {
        [Fact]
        public void OutgoingMessageAreReadFromTheRootContextTest()
        {
            var outgoingMessage = new object();

            var rootContext = new ChainExecutionContext();
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

            var rootContext = new ChainExecutionContext();
            using (var inner = rootContext.Push(context =>
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
                }
            }
            

        } 
    }
}