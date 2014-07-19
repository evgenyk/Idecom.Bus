namespace Idecom.Bus.Transport
{
    public class TransportMessageFinishedEventArgs
    {
        readonly TransportMessage _transportMessage;

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