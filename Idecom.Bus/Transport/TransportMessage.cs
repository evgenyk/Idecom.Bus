namespace Idecom.Bus.Transport
{
    using Addressing;

    public class TransportMessage
    {
        private readonly MessageIntent _intent;
        private readonly object _message;
        private readonly Address _sourceAddress;
        private readonly Address _targetAddress;

        public TransportMessage(object message, Address sourceAddress, Address targetAddress, MessageIntent intent) : this(message)
        {
            _sourceAddress = sourceAddress;
            _targetAddress = targetAddress;
            _intent = intent;
        }

        private TransportMessage(object message)
        {
            _message = message;
        }

        public Address SourceAddress
        {
            get { return _sourceAddress; }
        }

        public Address TargetAddress
        {
            get { return _targetAddress; }
        }

        public MessageIntent Intent
        {
            get { return _intent; }
        }

        public object Message
        {
            get { return _message; }
        }
    }
}