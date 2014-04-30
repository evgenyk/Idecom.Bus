namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    using System;
    using Implementations;
    using Interfaces;
    using Interfaces.Addons.Stories;
    using SampleMessages;

    //            .OtherwiseRaise<ITiredWaitingForAReply>();
    //!!!!!!!!!!!!!!!!!!!! chaining sagas together for complex flows!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!! SAGAS MUST BE TRANBSPARENT TO USE.
    /// <summary>
    ///     Bus.Expect<SayHeelloMessage>.For(10.Seconds)
    /// </summary>
    public class NicePersonConversationStory : Story<NicePersonConversationState>,
                                               IStartThisStoryWhenReceive<IMetAFriendEvent>,
                                               IHandle<SayHelloCommand>,
                                               ITimeoutFor<ITiredWaitingForAReply>
    {
        public void Handle(SayHelloCommand command)
        {
            Console.WriteLine("Said goodbuy");
            Bus.Reply(new SayGoodByeCommand("See you"));
            CloseStory();
        }

        public void Handle(IMetAFriendEvent command)
        {
            Console.WriteLine("Met a friend");
            Bus.Send(new SayHelloCommand("Hi"));
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