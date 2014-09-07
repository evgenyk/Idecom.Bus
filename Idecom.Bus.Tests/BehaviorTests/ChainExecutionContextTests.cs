namespace Idecom.Bus.Tests.BehaviorTests
{
    using Interfaces.Behaviors;
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
    }
}