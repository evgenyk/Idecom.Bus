namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    using System.Threading;
    using Implementations;
    using Interfaces;
    using log4net;
    using SampleMessages;

    [SingleInstanceSaga]
    public class NicePersonConversationSaga : Saga<NicePersonConversationState>,
                                              IStartThisSagaWhenReceive<IMetAFriendEvent>,
                                              IHandle<SayHelloCommand>,
                                              IHandle<SeeYouCommand>
    {
        readonly ILog _log = LogManager.GetLogger("NicePersonConversationSaga");

        public void Handle(SayHelloCommand command)
        {
            _log.Debug("04 \t Said goodbuy to a friend");
            Bus.Reply(new SayGoodByeCommand("See you"));
        }

        public void Handle(SeeYouCommand command)
        {
            _log.Debug(Thread.CurrentThread.ManagedThreadId + " \t 06 \t Received See you back");
            Data.Started = false;
            CloseSaga();
            _log.Debug("07 \t Closed saga");
        }

        public void Handle(IMetAFriendEvent command)
        {
            if (Data.Started)
            {
                _log.Debug("Saga already started while it shouldn't have been");
                return;
            }
            Data.Started = true;
            _log.Debug("02 \t Met a friend, said hi");
            var sayHelloCommand = new SayHelloCommand("Hi");
            Bus.Reply(sayHelloCommand);
            Data.FriendSaidHello = true;
        }
    }
}