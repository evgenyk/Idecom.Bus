using Idecom.Bus.Transport;

namespace Idecom.Bus.Interfaces
{
    public interface IMessageContext
    {
        TransportMessage TransportMessage { get; }
        int Atempt { get; }
        int MaxAttempts { get; }
    }
}