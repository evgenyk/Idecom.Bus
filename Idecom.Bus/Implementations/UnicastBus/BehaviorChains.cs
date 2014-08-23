namespace Idecom.Bus.Implementations.UnicastBus
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Behaviors;
    using Interfaces.Behaviors;
    using Internal.Behaviors;


    public enum ChainIntent
    {
        Send,
        SendLocal,
        Reply,
        Publish,
        Receive
    }
    
    public interface IBehaviorChains
    {
        IBehaviorChain GetChainFor(ChainIntent intent);
    }

    class BehaviorChains : IBehaviorChains
    {
        readonly Dictionary<ChainIntent, BehaviorChain> _chains;

        public BehaviorChains()
        {
            _chains = new Dictionary<ChainIntent, BehaviorChain>
                      {
                          {
                              ChainIntent.Send,
                              new BehaviorChain()
                              .WrapWith<TransportSendBehavior>()
                              .WrapWith<OutgoingMessageValidationBehavior>()
                          },
                          {
                              ChainIntent.SendLocal,
                              new BehaviorChain()
                              .WrapWith<TransportSendLocalBehavior>()
                              .WrapWith<OutgoingMessageValidationBehavior>()
                          },
                          {
                              ChainIntent.Publish,
                              new BehaviorChain()
                              .WrapWith<TransportPublishBehavior>()
                              .WrapWith<OutgoingMessageValidationBehavior>()
                          },
                          {
                              ChainIntent.Receive,
                              new BehaviorChain()
                              .WrapWith<DispachMessageToHandlerBehavior>()
                              .WrapWith<DispatcherMessageSagaBehavior>()
                          },
                      };
            
            //wrap everyting with start bus check
            foreach (var chain in _chains) { chain.Value.WrapWith<ThrowIfBusNotStartedBehavior>(); }
        }

        [DebuggerStepThrough]
        public IBehaviorChain GetChainFor(ChainIntent intent)
        {
            BehaviorChain chain;
            var tryGetValue = _chains.TryGetValue(intent, out chain);
            return tryGetValue ? chain : new BehaviorChain();
        }
    }
}