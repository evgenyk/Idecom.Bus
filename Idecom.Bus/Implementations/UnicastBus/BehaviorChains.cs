namespace Idecom.Bus.Implementations.UnicastBus
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Behaviors;
    using Interfaces.Behaviors;
    using Internal.Behaviors;
    using Transport;

    public interface IBehaviorChains
    {
        IBehaviorChain GetChainFor(MessageIntent intent);
    }

    class BehaviorChains : IBehaviorChains
    {
        readonly Dictionary<MessageIntent, BehaviorChain> _chains;

        public BehaviorChains()
        {
            _chains = new Dictionary<MessageIntent, BehaviorChain>
                      {
                          {
                              MessageIntent.Send,
                              new BehaviorChain()
                              .WrapWith<TransportSendBehavior>()
                              .WrapWith<OutgoingMessageValidationBehavior>()
                          },
                          {
                              MessageIntent.SendLocal,
                              new BehaviorChain()
                              .WrapWith<TransportSendLocalBehavior>()
                              .WrapWith<OutgoingMessageValidationBehavior>()
                          },
                          {
                              MessageIntent.Publish,
                              new BehaviorChain()
                              .WrapWith<TransportPublishBehavior>()
                              .WrapWith<OutgoingMessageValidationBehavior>()
                          },
                      };
        }

        [DebuggerStepThrough]
        public IBehaviorChain GetChainFor(MessageIntent intent)
        {
            BehaviorChain chain;
            var tryGetValue = _chains.TryGetValue(intent, out chain);
            return tryGetValue ? chain : new BehaviorChain();
        }
    }
}