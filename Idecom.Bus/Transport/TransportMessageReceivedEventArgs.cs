namespace Idecom.Bus.Transport
{
    using System.Diagnostics;

    public class TransportMessageReceivedEventArgs
    {
        readonly int _attempt;
        readonly int _maxRetries;
        readonly TransportMessage _transportMessage;

        [DebuggerStepThrough]
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