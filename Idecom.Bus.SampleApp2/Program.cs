﻿namespace Idecom.Bus.SampleApp2
{
    using System;
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
        static void Main(string[] args)
        {
            var container = new WindsorContainer();
            var busInstance = Configure.With()
                                       .WindsorContainer(container)
                                       .Log4Net()
                                       .MongoDbTransport("mongodb://localhost", "messageHub")
                                       .JsonNetSerializer()
                                       .RouteMessagesFromNamespaceTo<SayHelloCommand>("app1")
                                       .MongoPublisher("mongodb://localhost", "messageHub")
                                       .CreateBus("app2");

            var bus = busInstance.Start();
            Console.WriteLine("01 \t Sending IMetAFriendEvent");

            for (int i = 1; i < 100; i++) 
            {
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