namespace Idecom.Bus.Interfaces
{
    using Transport;

    public interface ITransport
    {
        int WorkersCount { get; }
        void Send(TransportMessage transportMessage);
    }
}