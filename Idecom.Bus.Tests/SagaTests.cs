using Castle.Windsor;
using Idecom.Bus.Implementations;
using Idecom.Bus.IoC.CastleWindsor;
using Idecom.Bus.PubSub.MongoDB;
using Idecom.Bus.Serializer.JsonNet;
using Idecom.Bus.Transport.MongoDB;

namespace Idecom.Bus.Tests
{
    using Xunit;

    public class SagaTests
    {
        [Fact]
        public void CanHaveASagaEndToEndTest()
        {
//            var busInstance = Configure.With()
//                                       .WindsorContainer()
//                                       .MongoDbTransport("mongodb://localhost", "messageHub")
//                                       .JsonNetSerializer()
//                                       .RouteMessagesFromNamespaceTo<SayHelloCommand>("app2")
//                                       .MongoPublisher("mongodb://localhost", "messageHub")
//                                       .CreateBus("app1")
//                                       .Start();


        }


        class MemoryTransport
        {
             
        }

        class MemorySagaPersister
        {
             
        }

        class MemorySubscriptionStorage
        {
             
        }

    }

}