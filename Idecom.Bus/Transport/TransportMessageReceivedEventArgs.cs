namespace Idecom.Bus.Transport
{
    public class TransportMessageReceivedEventArgs
    {
        private readonly int _attempt;
        private readonly int _maxRetries;
        private readonly TransportMessage _transportMessage;

        public TransportMessageReceivedEventArgs(TransportMessage transportMessage, int attempt, int maxRetries)
        {
            _transportMessage = transportMessage;
            _attempt = attempt;
            _maxRetries = maxRetries;
        }

        public TransportMessage TransportMessage
        {
            get { return _transportMessage; }
        }

        public int Attempt
        {
            get { return _attempt; }
        }

        public int MaxRetries
        {
            get { return _maxRetries; }
        }
    }
}