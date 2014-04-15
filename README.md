Idecom.Bus - service bus for the working class
--------
Service Bus with support of pluggable transports, serializers and containers

Installation
-------
You can install Idecom.Bus into Idecom.Host or host it in your own application.

Install NuGet package(s) from NuGet:

````
Install-Package Idecom.Bus //for the barebone bus
or
Install-Package Idecom.Bus.Implementation //meta package includes: Idecom.Bus, Idecom.Bus.IoC.CastleWindsor, Idecom.Bus.Serializer.JsonNet and Idecom.Bus.Transport.MongoDB
````

The following plugin packages are currently available:

* Idecom.Bus.Transport.MongoDB
* Idecom.Bus.Serializer.JsonNet
* Idecom.Bus.IoC.CastleWindsor

Configuration
-----------
````csharp
_bus = Configure.With()
    .WindsorContainer(_container)
    .MongoDbTransport("mongodb://localhost", "messageHub")
    .JsonNetSerializer()
    .RouteMessagesFromNamespaceTo<SayHelloMessage>("receiver")
    .CreateBus("sender", 1, 2) //Number of worker threads as well as number of retries
    .Start();
````

Unit-of-work support
------

You can provide a lambda which would be called before start of processing of each message where you can do transaction/unit-of-work initialization.
The lambda has to return IDisposable which would be disposed just after message has been processed/failed.

````csharp
var bus = Configure.With()
                   .WindsorContainer(_container, () => _container.BeginScope())
...
````
Tracking retries and preventing messages from going to the error queue
----
You can check which message attempt is currently handled by inspecting bus.CurrentMessageContext. There are two properties which might be of interest:

````csharp
public interface IMessageContext
{
  TransportMessage TransportMessage { get; }
  int Atempt { get; }
  int MaxAttempts { get; }
}
````

The first one is the current attempt which starts from 1 on the first message receive and increased with each subsequent attempt.

The MaxAttempts would help you to figure out whether this is the last attempts if you want to prevent message to go to the error queue and respond with the failure to the external component.

Message handling logic, service restarts and transactions
-----
Idecom.Bus doesn't provide support for transactions out-of-the-box so you need to implement your own using the UnitOfWork via container.

Each message being received in the order they are written to the database (with MongoDB transport messages are ordered by MongoID).

You can have multiple services listening on the same queue for the load balancing scenarios.

On push to the message is marked as "Pending" aka ready to be processed.

MongoDB transport is using FindAndModify MongoDB command to peek the message & sets it's status to "Received" as well as setting the receivedBy and tine of receive.

Each message handling is wrapped in it's own UnitOfWork provided to container and the appropriate message handler is being called.

Of exception the retry logic is executed. If retry cound has been exhausted the message is marked as "Failed".
Service Restarts handling

If the service is being re-started anywhere during the processing loop the messages that are marked as receivedBy the current consumer are returned back to the queue by findAndModify command.

If the service never comes back the messages would stuck in the queue as received but there would be noone to release them.

There is a need to implement a timeout service which would release messages which are too long in the received state.
 
Idecom.Host integration
----

Startup configuration

````csharp
public override bool Start(HostControl hostControl)
{
    Log.Info("SampleService Starting...");
    hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(10));
    _bus = Configure.With()
        .WindsorContainer(_container)
        .MongoDbTransport("mongodb://localhost", "messageHub")
        .JsonNetSerializer()
        .RouteMessagesFromNamespaceTo<SayHelloMessage>("receiver")
        .CreateBus("sender", 1, 2)
        .Start();
    Log.Info("SampleService Started");
    return true;
}  
 
public override bool Stop(HostControl hostControl)
{
    Log.Info("SampleService Stopped");
    hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(10));
    _bus.Stop();
    return true;
}
````

Message handler

````csharp
public class MessageHandler : IHandleMessage<SayHelloMessage>
{
    public IBus Bus { get; set; }
    public void Handle(SayHelloMessage message)
    {
        Console.WriteLine("received message: " + message.Hello);
        Bus.Reply(new SayHelloMessage("Hello back!"));
    }
}
````

Async runner
----

````csharp
public class Start: IWantToStartAfterServiceStarts
{
    public IBus Bus { get; set; }
    public void AfterStart()
    {
        Bus.Send(new SayHelloMessage("Hellow from receiver"));
    }
    public void BeforeStop()
    {
    }
}
````
