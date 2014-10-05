namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    using System;
    using System.Threading;
    using Implementations;
    using Interfaces;
    using SampleMessages;

    [SingleInstanceSaga]
    public class NicePersonConversationSaga : Saga<NicePersonConversationState>,
                                              IStartThisSagaWhenReceive<IMetAFriendEvent>,
                                              IHandle<SayHelloCommand>,
                                              IHandle<SeeYouCommand>
    {
        public void Handle(SayHelloCommand command)
        {
            Console.WriteLine("04 \t Said goodbuy to a friend");
            Bus.Reply(new SayGoodByeCommand("See you"));
        }

        public void Handle(SeeYouCommand command)
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " \t 06 \t Received See you back");
            Data.Started = false;
            CloseSaga();
            Console.WriteLine("07 \t Closed saga");
        }

        public void Handle(IMetAFriendEvent command)
        {
            if (Data.Started)
            {
                Console.WriteLine("Saga already started while it shouldn't have been");
                return;
            }
            Data.Started = true;
            Console.WriteLine("02 \t Met a friend, said hi");
            var sayHelloCommand = new SayHelloCommand("Hi");
            Bus.Reply(sayHelloCommand);
            Data.FriendSaidHello = true;
        }
    }
}