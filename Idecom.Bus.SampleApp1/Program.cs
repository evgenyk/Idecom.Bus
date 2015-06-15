namespace Idecom.Bus.SampleApp1
{
    using System;
    using System.Collections;
    using Castle.Windsor;
    using Implementations;
    using Interfaces;
    using IoC.CastleWindsor;
    using Logging.Log4Net;
    using PubSub.MongoDB;
    using SampleMessages;
    using Serializer.JsonNet;
    using Transport.MongoDB;
    using Transport.RabbitMq;

    class Program
    {
        static void Main()
        {
            /*
            var host = Host
                .InfrustructureFolder("")
                .HostedServiceFolder("")
                .Start();
                */

            log4net.Config.XmlConfigurator.Configure();

            var container = new WindsorContainer();
            IBusInstance busInstance = Configure.With()
                                       .WindsorContainer(container)
                                       .Log4Net()
                                       //.RabbitMqTransport("localhost")
                                       .MongoDbTransport("mongodb://localhost", "messageHub", 1)
                                       .JsonNetSerializer() // should be automatic
                                       .RouteMessagesFromNamespaceTo<SayHelloCommand>("app2") // must be automatic
                                       .MongoPublisher("mongodb://localhost", "messageHub")
                                       .CreateBus("app1")
                                       .Start();

            var sayHelloCommand = new SayHelloCommand("Blah");
            busInstance.SendLocal(sayHelloCommand);

            Console.WriteLine("Bus configured. Press any key to close the app.");
            Console.ReadKey();
            busInstance.Stop();
        }
    }
}