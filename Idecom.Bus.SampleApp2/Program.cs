using System.Threading;

namespace Idecom.Bus.SampleApp2
{
    using System;
    using Castle.Windsor;
    using Implementations;
    using IoC.CastleWindsor;
    using PubSub.MongoDB;
    using SampleMessages;
    using Serializer.JsonNet;
    using Transport.MongoDB;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var container = new WindsorContainer();
            var busInstance = Configure.With()
                                       .WindsorContainer(container)
                                       .MongoDbTransport("mongodb://localhost", "messageHub")
                                       .JsonNetSerializer()
                                       .RouteMessagesFromNamespaceTo<SayHelloCommand>("app1")
                                       .MongoPublisher("mongodb://localhost", "messageHub")
                                       .CreateBus("app2");

            var bus = busInstance.Start();
            bus.Raise<IMetAFriendEvent>(x =>
            {
                x.Name = "sdfsdfs";
                x.Uri = new Uri("http://www.com");
            });

//            for (int i = 0; i < 10; i++)
//            {
//                int i1 = i;
//                bus.Raise<IMetAFriendEvent>(x => { x.Name = "sdfsdfs " + i1; });
//            }


            Console.WriteLine("Bus configured. Press any key to close the app.");
            Console.ReadKey();
            bus.Stop();
        }
    }
}