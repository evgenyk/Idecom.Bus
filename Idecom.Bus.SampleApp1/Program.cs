using System;
using Castle.Windsor;
using Idecom.Bus.Implementations;
using Idecom.Bus.IoC.CastleWindsor;
using Idecom.Bus.PubSub.MongoDB;
using Idecom.Bus.SampleMessages;
using Idecom.Bus.Serializer.JsonNet;
using Idecom.Bus.Transport.MongoDB;

namespace Idecom.Bus.SampleApp1
{
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
                                       .MongoPublisher("mongodb://localhost", "messageHub")
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