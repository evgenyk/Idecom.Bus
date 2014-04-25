using System;
using Castle.Windsor;
using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
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
            IBusInstance busInstance = Configure.With()
                .WindsorContainer(container)
                .MongoDbTransport("mongodb://localhost", "messageHub")
                .JsonNetSerializer()
                .RouteMessagesFromNamespaceTo<SayHelloMessage>("app2")
                .PubSub("mongodb://localhost", "messageHub")
                .CreateBus("app1", 1, 2);

            IBusInstance bus1 = busInstance.Start();

//            bus1.SendLocal(new SayHelloMessage("Hello local 1"));
//            bus1.Send(new SayHelloMessage("Hello local 2"));
//                        for (var i = 0; i < 1000000; i++)
//            {
//                PerfMetric.Received++;
//                bus1.Send(new SayHelloMessage("Hello local: " + DateTime.Now));
//            }

            Console.WriteLine("Bus configured. Press any key to close the app.");
            Console.ReadKey();
            bus1.Stop();
        }
    }
}