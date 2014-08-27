namespace Idecom.Bus.Interfaces
{
    using System;
    using Implementations;

    public interface IMessageToStartSagaMapping : IRoutingTable<Type>
    {
    }
}