namespace Idecom.Bus.Interfaces
{
    using System;
    using Transport;

    public interface ITransport
    {
        void Send(TransportMessage transportMessage, bool isProcessingIncommingMessage, Action<TransportMessage> delayMessageAction);
    }
}