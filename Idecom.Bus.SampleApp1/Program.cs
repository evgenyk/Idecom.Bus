namespace Idecom.Bus.SampleApp1
{
    using System;
    using Castle.Windsor;
    using Implementations;
    using Interfaces;
    using IoC.CastleWindsor;
    using log4net.Config;
    using Logging.Log4Net;
    using PubSub.MongoDB;
    using SampleMessages;
    using Serializer.JsonNet;
    using Transport.MongoDB;

    class Program
    {
        static void Main()
        {
            XmlConfigurator.Configure();

            IBusInstance busInstance = Configure.WithContainer()
                                       .WindsorContainer()
                                       .Log4Net()
                                       .MongoDbTransport("mongodb://localhost", "messageHub", 1)
                                       .JsonNetSerializer() // should be automatic
                                       .RouteMessagesFromNamespaceTo<SayHelloCommand>("app2") // must be automatic
                                       .MongoPublisher("mongodb://localhost", "messageHub")
                                       .CreateInstance()
                                       .Start();

            //Auto.Configure().Start();

            var sayHelloCommand = new SayHelloCommand("Blah");
            busInstance.SendLocal(sayHelloCommand);

            Console.WriteLine("Bus configured. Press any key to close the app.");
            Console.ReadKey();
            busInstance.Stop();
        }
    }
}