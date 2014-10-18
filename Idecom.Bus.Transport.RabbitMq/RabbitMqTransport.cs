namespace Idecom.Bus.Transport.RabbitMq
{
    using System;
    using Interfaces;

    public class RabbitMqTransport : ITransport, IBeforeBusStarted, IBeforeBusStopped
    {
        public string RabbitHost { get; set; }

        public void Send(TransportMessage transportMessage, bool isProcessingIncommingMessage, Action<TransportMessage> delayMessageAction)
        {
        }

        public void BeforeBusStarted()
        {
        }

        public void BeforeBusStopped()
        {
        }
    }
}