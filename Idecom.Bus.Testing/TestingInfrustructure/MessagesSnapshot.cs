namespace Idecom.Bus.Testing.TestingInfrustructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MessagesSnapshot : IMessagesSnapshot
    {
        readonly IList<IMessageWithTelemetry> _incomingMessages;
        readonly IList<IBehaviorInvocation> _behaviorInvocation;

        public MessagesSnapshot()
        {
            _incomingMessages = new List<IMessageWithTelemetry>();
            _behaviorInvocation = new List<IBehaviorInvocation>();
        }

        public void HasBeenHandled<TMessage, THandler>(int numberOfTimes = 1)
        {
            var hasBeenHandled = _incomingMessages.Any(x => x.MessageType == typeof(TMessage) && x.Handler.GetType() == typeof(THandler));
            
            if (!hasBeenHandled)
                throw new Exception(string.Format("Message {0} was not handled by handler {1} in this session", typeof (TMessage).Name, typeof (THandler).Name));
        }

        public void Push(IMessageWithTelemetry messageInfo)
        {
            _incomingMessages.Add(messageInfo);
        }

        public void Push(IBehaviorInvocation behaviorInfo)
        {
            _behaviorInvocation.Add(behaviorInfo);
        }
    }
}