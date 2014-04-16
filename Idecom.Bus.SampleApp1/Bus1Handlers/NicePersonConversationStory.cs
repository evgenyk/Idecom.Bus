using System.ComponentModel;
using System.Threading;
using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.SampleMessages;

namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    public class NicePersonConversationStory : Story<NicePersonConversationState>,
        IStartThisStoryWhenReceive<MetAFriendMessage>,
        IHandleMessage<SayHelloMessage>,
        ITimeoutFor<ITiredWaitingForAReply>
    {
        public void Handle(MetAFriendMessage message)
        {
            Bus.Send(new SayHelloMessage("Hi"));
            Bus.Expect<SayHelloMessage>.For(10.Seconds)
                .OtherwiseRaise<ITiredWaitingForAReply>();

            //!!!!!!!!!!!!!!!!!!!! chaining sagas together for complex flows!!!!!!!!!!!!!!!!!
        }

        public void Handle(SayHelloMessage message)
        {
            Bus.Reply(new SayGoodByeMessage("See you"));
            CloseStory();
        }

        public void HandleTimeout(ITiredWaitingForAReply timeout)
        {
            throw new System.NotImplementedException();
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
