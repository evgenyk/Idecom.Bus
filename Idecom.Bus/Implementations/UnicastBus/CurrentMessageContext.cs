namespace Idecom.Bus.Implementations.UnicastBus
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using Transport;

    public class CurrentMessageContext : IMessageContext
    {
        private readonly IList<Action> _delayedActions;

        public CurrentMessageContext()
        {
            _delayedActions = new List<Action>();
        }

        public IEnumerable<Action> DelayedActions
        {
            get { return _delayedActions; }
        }

        public TransportMessage TransportMessage { get; set; }

        public int Attempt { get; set; }
        public int MaxAttempts { get; set; }

        public void DelayedSend(Action action)
        {
            _delayedActions.Add(action);
        }
    }
}