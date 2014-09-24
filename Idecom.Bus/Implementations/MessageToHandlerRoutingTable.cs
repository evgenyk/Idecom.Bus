namespace Idecom.Bus.Implementations
{
    using System.Reflection;
    using Interfaces;

    public class MessageToHandlerRoutingTable : PluralRoutingTable<MethodInfo>, IMessageToHandlerRoutingTable
    {
    }
}