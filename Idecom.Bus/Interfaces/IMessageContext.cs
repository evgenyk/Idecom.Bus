using Idecom.Bus.Transport;

namespace Idecom.Bus.Interfaces
{
    public interface IMessageContext
    {
        TransportMessage TransportMessage { get; }
        int Attempt { get; }
        int MaxAttempts { get; }
    }
}