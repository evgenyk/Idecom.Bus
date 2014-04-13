namespace Idecom.Bus.Transport
{
    public class TransportMessageFinishedEventArgs
    {
        private readonly TransportMessage _transportMessage;

        public TransportMessageFinishedEventArgs(TransportMessage transportMessage)
        {
            _transportMessage = transportMessage;
        }

        public TransportMessage TransportMessage
        {
            get { return _transportMessage; }
        }
    }
}