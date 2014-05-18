﻿namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    using System;
    using Implementations;
    using Interfaces;
    using Interfaces.Addons.Stories;
    using SampleMessages;

    //            .OtherwiseRaise<ITiredWaitingForAReply>();
    //!!!!!!!!!!!!!!!!!!!! chaining sagas together for complex flows!!!!!!!!!!!!!!!!!
    //!!!!!!!!!!!!!!!!!!!! SAGAS MUST BE TRANSPARENT TO USE.
    /// <summary>
    ///     Bus.Expect<SayHeelloMessage>.For(10.Seconds)
    /// </summary>
    public class NicePersonConversationSaga : Saga<NicePersonConversationState>,
                                               IStartThisSagaWhenReceive<IMetAFriendEvent>,
                                               IHandle<SayHelloCommand>,
                                               ITimeoutFor<ITiredWaitingForAReply>
    {
        public void Handle(SayHelloCommand command)
        {
            Console.WriteLine("Said goodbuy");
            Bus.Reply(new SayGoodByeCommand("See you"));
            CloseSaga();
        }

        public void Handle(IMetAFriendEvent command)
        {
            Console.WriteLine("Met a friend");
            var sayHelloCommand = new SayHelloCommand("Hi");
            Bus.Send(sayHelloCommand);
            Data.FriendSaidHello = true;
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

    public class NicePersonConversationState : ISagaState
    {
        public bool FriendSaidHello { get; set; }
        public bool RepliedBackWithHello { get; set; }

    }
}