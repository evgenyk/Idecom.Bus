namespace Idecom.Bus.Implementations
{
    using System;
    using Interfaces;

    public class MessageToStartSagaMapping : RoutingTable<Type>, IMessageToStartSagaMapping
    {
    }
}