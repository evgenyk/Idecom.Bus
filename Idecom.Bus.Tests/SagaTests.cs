using System;
using System.Collections.Generic;
using Castle.Windsor;
using Idecom.Bus.Addressing;
using Idecom.Bus.Implementations;
using Idecom.Bus.Implementations.UnicastBus;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.PubSub;
using Idecom.Bus.IoC.CastleWindsor;
using Idecom.Bus.PubSub.MongoDB;
using Idecom.Bus.Serializer.JsonNet;
using Idecom.Bus.Transport;
using Idecom.Bus.Transport.MongoDB;

namespace Idecom.Bus.Tests
{
    using Xunit;

    public class SagaTests: IHandle<BlahCommand>
    {
        [Fact]
        public void CanHaveASagaEndToEndTest()
        {
            var bus = Configure.With()
                .WindsorContainer()
                .InMemoryTransport()
                .JsonNetSerializer()
                .CreateBus("app1")
                .Start()
                ;
            bus.SendLocal(new BlahCommand());
//                                       .RouteMessagesFromNamespaceTo<SayHelloCommand>("app2")
//                                       .MongoPublisher("mongodb://localhost", "messageHub")
//                                       .CreateBus("app1")
//                                       .Start();


        }


        public class InMemoryTransport: ITransport
        {
            public int WorkersCount { get; set; }
            public int Retries { get; set; }

            public void ChangeWorkerCount(int workers)
            {
            }

            public void Send(TransportMessage transportMessage, CurrentMessageContext currentMessageContext = null)
            {
            }

            public event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
            public event EventHandler<TransportMessageFinishedEventArgs> TransportMessageFinished;
        }

        public class InMemorySagaPersister: ISagaStorage
        {
            public void Update(string sagaId, object sagaData)
            {
            }

            public object Get(string sagaId)
            {
                return null;
            }

            public void Close(string sagaId)
            {
            }
        }

        class InMemorySubscriptionStorage: ISubscriptionStorage
        {
            public IEnumerable<Address> GetSubscribersFor<T>() where T : class
            {
                yield break;
            }

            public void Subscribe(Address subscriber, Type type)
            {
            }

            public void Unsubscribe(Address subscriber, Type type)
            {
            }
        }

        public void Handle(BlahCommand command)
        {

        }
    }

    public class BlahCommand
    {
    }

    public static class InMemory
    {
        public static Configure InMemoryTransport(this Configure configure, int workersCount = 1, int retries = 1)
        {
            configure.Container.Configure<SagaTests.InMemoryTransport>(ComponentLifecycle.Singleton);
            configure.Container.ConfigureProperty<SagaTests.InMemoryTransport>(x => x.WorkersCount, workersCount);
            configure.Container.ConfigureProperty<SagaTests.InMemoryTransport>(x => x.Retries, retries);

            return configure;
        }


    }


}