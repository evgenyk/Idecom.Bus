namespace Idecom.Bus.SampleApp2.Handlers
{
    using Annotations;
    using Implementations;
    using Interfaces;
    using Interfaces.Addons.Sagas;
    using log4net;
    using SampleMessages;

    [UsedImplicitly]
    public class SayHelloApp2Saga : Saga<SayHelloSagaInApp2State>,
                                    IStartThisSagaWhenReceive<SayHelloCommand>,
                                    IHandle<SayGoodByeCommand>
    {
        readonly log4net.ILog _log = LogManager.GetLogger("SayHelloApp2Saga");

        public void Handle(SayGoodByeCommand command)
        {
            _log.Debug("05 \t A friend said good bye, said see you");
            Bus.Reply(new SeeYouCommand("See you"));
            
            CloseSaga();
            _log.Debug("05 \t Closed saga");
        }

        public void Handle(SayHelloCommand command)
        {
            _log.Debug("03 \t A friend said hello");

            Bus.Reply(new SayHelloCommand("Hello back to you!!"));
        }
    }

    public class SayHelloSagaInApp2State : ISagaState
    {
    }
}