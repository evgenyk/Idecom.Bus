namespace Idecom.Bus.Interfaces
{
    using Transport;

    public interface IMessageContext
    {
        TransportMessage IncomingTransportMessage { get; }
        int Attempt { get; }
        int MaxAttempts { get; }
    }
}