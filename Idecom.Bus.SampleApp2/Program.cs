namespace Idecom.Bus.SampleApp2
{
    using System;
    using Castle.Windsor;
    using Implementations;
    using IoC.CastleWindsor;
    using log4net;
    using Logging.Log4Net;
    using PubSub.MongoDB;
    using SampleMessages;
    using Serializer.JsonNet;
    using Transport.MongoDB;

    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var container = new WindsorContainer();
            var busInstance = Configure.With()
                                       .WindsorContainer(container)
                                       .Log4Net()
                                       .MongoDbTransport("mongodb://localhost", "messageHub", 4)
                                       .JsonNetSerializer()
                                       .RouteMessagesFromNamespaceTo<SayHelloCommand>("app1")
                                       .MongoPublisher("mongodb://localhost", "messageHub")
                                       .CreateBus("app2");

            var bus = busInstance.Start();

            var log = LogManager.GetLogger("Program");
            log.Debug("01 \t Sending IMetAFriendEvent");

            for (int i = 1; i < 1000; i++)
            {
                if (i % 1000 == 0)
                {
                    log.Debug(i);
                }
                bus.Raise<IMetAFriendEvent>(x =>
                {
                    x.Name = "sdfsdfs";
                    x.Uri = new Uri("http://www.com");
                });
            }


            Console.WriteLine("Bus configured. Press any key to close the app.");
            Console.ReadKey();
            bus.Stop();
        }
    }
}