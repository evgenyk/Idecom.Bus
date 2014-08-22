namespace Idecom.Bus.Interfaces
{
    using Addressing;
    using Implementations;

    public interface IMessageToEndpointRoutingTable : IRoutingTable<Address>
    {
    }
}