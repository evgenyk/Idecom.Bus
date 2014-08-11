namespace Idecom.Bus.Implementations.UnicastBus
{
    using Transport;

    public class OutgoingMessageContext
    {
        public TransportMessage OutgoingMessage { get; set; }
    }
}