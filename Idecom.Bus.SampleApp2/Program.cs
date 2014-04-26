using System;
using System.Linq.Expressions;
using Castle.Windsor;
using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.IoC.CastleWindsor;
using Idecom.Bus.PubSub.MongoDB;
using Idecom.Bus.SampleMessages;
using Idecom.Bus.Serializer.JsonNet;
using Idecom.Bus.Transport.MongoDB;

namespace Idecom.Bus.SampleApp2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var container = new WindsorContainer();
            IBusInstance busInstance = Configure.With()
                .WindsorContainer(container)
                .MongoDbTransport("mongodb://localhost", "messageHub")
                .JsonNetSerializer()
                .RouteMessagesFromNamespaceTo<SayHelloMessage>("app1")
                .PubSub("mongodb://localhost", "messageHub")
                .CreateBus("app2");

            IBusInstance bus = busInstance.Start();
            bus.Raise<IMetAFriendEvent>(x => { x.Name = "sdfsdfs"; });

            Expression<Func<int>> ff = () => 12;


            Console.WriteLine("Bus configured. Press any key to close the app.");
            Console.ReadKey();
            bus.Stop();
        }
    }
}