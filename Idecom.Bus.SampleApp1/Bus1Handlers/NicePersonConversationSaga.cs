namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    using System;
    using Implementations;
    using Interfaces;
    using SampleMessages;

    public class NicePersonConversationSaga : Saga<NicePersonConversationState>,
                                               IStartThisSagaWhenReceive<IMetAFriendEvent>,
                                               IHandle<SayHelloCommand>
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
    }

}