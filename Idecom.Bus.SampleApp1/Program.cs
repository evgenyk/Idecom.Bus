namespace Idecom.Bus.SampleApp1
{
    using System;
    using System.Collections;
    using Castle.Windsor;
    using Implementations;
    using IoC.CastleWindsor;
    using Logging.Log4Net;
    using PubSub.MongoDB;
    using SampleMessages;
    using Serializer.JsonNet;
    using Transport.MongoDB;

    class Program
    {
        static void Main()
        {
            ICollection config = log4net.Config.XmlConfigurator.Configure();

            var container = new WindsorContainer();
            var busInstance = Configure.With()
                                       .WindsorContainer(container)
                                       .Log4Net()
                                       .MongoDbTransport("mongodb://localhost", "messageHub")
                                       .JsonNetSerializer()
                                       .RouteMessagesFromNamespaceTo<SayHelloCommand>("app2")
                                       .MongoPublisher("mongodb://localhost", "messageHub")
                                       .CreateBus("app1")
                                       .Start();

            //var sayHelloCommand = new SayHelloCommand("Blah");
            //busInstance.SendLocal(sayHelloCommand);

            Console.WriteLine("Bus configured. Press any key to close the app.");
            Console.ReadKey();
            busInstance.Stop();
        }
    }
}