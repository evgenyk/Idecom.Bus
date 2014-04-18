using System;
using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.SampleMessages;

namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    /// <summary>
    ///            Bus.Expect<SayHeelloMessage>.For(10.Seconds)
    //            .OtherwiseRaise<ITiredWaitingForAReply>();
    //!!!!!!!!!!!!!!!!!!!! chaining sagas together for complex flows!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!! SAGAS MUST BE TRANBSPARENT TO USE.
    /// </summary>
    public class NicePersonConversationStory : Story<NicePersonConversationState>,
        IStartThisStoryWhenReceive<MetAFriendMessage>,
        IHandleMessage<SayHelloMessage>,
        ITimeoutFor<ITiredWaitingForAReply>
    {
        public void Handle(SayHelloMessage message)
        {
            Console.WriteLine("Said goodbuy");
            Bus.Reply(new SayGoodByeMessage("See you"));
            CloseStory();
        }

        public void Handle(MetAFriendMessage message)
        {
            Console.WriteLine("Met a friend");
            Bus.Send(new SayHelloMessage("Hi"));
        }

        public void HandleTimeout(ITiredWaitingForAReply timeout)
        {
            throw new NotImplementedException();
        }
    }

    public interface ITiredWaitingForAReply
    {
    }

    public interface ITimeoutFor<T>
    {
        void HandleTimeout(T timeout);
    }

    public class NicePersonConversationState : IStoryState
    {
    }
}