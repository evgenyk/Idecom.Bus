namespace Idecom.Bus.Testing.TestingInfrustructure
{
    using System.Collections.Generic;
    using System.Linq;

    public class MessagesSnapshot : IMessagesSnapshot
    {
        readonly IList<IMessageWithTelemetry> _incomingMessages;

        public MessagesSnapshot()
        {
            _incomingMessages = new List<IMessageWithTelemetry>();
        }

        public bool HasBeenHandled<T>(int numberOfTimes = 1)
        {
            return _incomingMessages.Select(telemetry => telemetry.Message).OfType<T>().Any();
        }

        public void Push(IMessageWithTelemetry messageInfo)
        {
            _incomingMessages.Add(messageInfo);
        }
    }
}