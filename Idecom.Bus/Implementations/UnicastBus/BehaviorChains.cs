namespace Idecom.Bus.Implementations.UnicastBus
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Behaviors;
    using Interfaces.Behaviors;
    using Internal.Behaviors;
    using Internal.Behaviors.Incoming;


    public enum ChainIntent
    {
        Send,
        SendLocal,
        Reply,
        Publish,
        TransportMessageReceive,
        IncomingMessageHandling
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
                              ChainIntent.Reply,
                              new BehaviorChain()
                              .WrapWith<ReplyBehavior>()
                              .WrapWith<OutgoingMessageValidationBehavior>()
                          },
                          {
                              ChainIntent.Publish,
                              new BehaviorChain()
                              .WrapWith<TransportPublishBehavior>()
                              .WrapWith<OutgoingMessageValidationBehavior>()
                          },
                          {
                              ChainIntent.TransportMessageReceive,
                              new BehaviorChain()
                              .WrapWith<MultiplexIncomingTransportMessageToHandlers>()
                          },                          
                          {
                              ChainIntent.IncomingMessageHandling,
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