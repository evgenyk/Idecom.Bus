using System.ComponentModel;
using Idecom.Bus.Implementations.UnicastBus;

namespace Idecom.Bus.SampleApp1
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
        private static void Main()
        {
            var container = new WindsorContainer();
            var busInstance = Configure.With()
                                       .WindsorContainer(container)
                                       .MongoDbTransport("mongodb://localhost", "messageHub")
                                       .JsonNetSerializer()
                                       .RouteMessagesFromNamespaceTo<SayHelloCommand>("app2")
                                       .PubSub("mongodb://localhost", "messageHub")
                                       .CreateBus("app1")
                                       .Start();


//            bus1.SendLocal(new SayHelloCommand("Hello local 1"));
//            bus1.Send(new SayHelloCommand("Hello local 2"));
//                        for (var i = 0; i < 1000000; i++)
//            {
//                PerfMetric.Received++;
//                bus1.Send(new SayHelloCommand("Hello local: " + DateTime.Now));
//            }

            Console.WriteLine("Bus configured. Press any key to close the app.");
            Console.ReadKey();
            busInstance.Stop();
        }
    }
}