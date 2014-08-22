namespace Idecom.Bus.Interfaces
{
    using System.Reflection;
    using Implementations;

    public interface IMessageToHandlerRoutingTable : IMultiRoutingTable<MethodInfo>
    {
    }
}