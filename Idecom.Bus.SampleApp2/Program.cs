using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.IoC.CastleWindsor.WindsorContainer;
using Idecom.Bus.SampleMessages;
using Idecom.Bus.Serializer.JsonNet.JsonNetSerializer;
using Idecom.Bus.Transport.MongoDB.MongoDbTransport;

namespace Idecom.Bus.SampleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new WindsorContainer();
            IBusInstance busInstance = Configure.With()
                .WindsorContainer(container)
                .MongoDbTransport("mongodb://localhost", "messageHub")
                .JsonNetSerializer()
                .RouteMessagesFromNamespaceTo<SayHelloMessage>("app1")
                .CreateBus("app2", 1, 2);

            IBusInstance bus = busInstance.Start();
            bus.Send(new MetAFriendMessage());


            Console.WriteLine("Bus configured. Press any key to close the app.");
            Console.ReadKey();
            bus.Stop();
        }
    }
}
