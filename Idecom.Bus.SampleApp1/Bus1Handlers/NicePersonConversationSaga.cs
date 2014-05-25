using System;
using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.SampleMessages;

namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    public class NicePersonConversationSaga : Saga<NicePersonConversationState>,
                                              IStartThisSagaWhenReceive<IMetAFriendEvent>,
                                              IHandle<SayHelloCommand>,
                                              IHandle<SeeYouCommand>
    {
        public void Handle(SayHelloCommand command)
        {
            Console.WriteLine("Said goodbuy to a friend");
            Bus.Reply(new SayGoodByeCommand("See you"));
        }

        public void Handle(IMetAFriendEvent command)
        {
            Console.WriteLine("Met a friend, said hi");
            var sayHelloCommand = new SayHelloCommand("Hi");
            Bus.Send(sayHelloCommand);
            Data.FriendSaidHello = true;
        }

        public void Handle(SeeYouCommand command)
        {
            Console.WriteLine("Received See you back");
            CloseSaga();
            Console.WriteLine("Closed saga");
        }
    }
}