namespace Idecom.Bus.Interfaces
{
    using Transport;

    public interface IMessageContext
    {
        TransportMessage TransportMessage { get; }
        int Attempt { get; }
        int MaxAttempts { get; }
    }
}