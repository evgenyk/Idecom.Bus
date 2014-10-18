namespace Idecom.Bus.SampleApp2.Handlers
{
    using Annotations;
    using Implementations;
    using Interfaces;
    using Interfaces.Addons.Sagas;
    using Interfaces.Logging;
    using SampleMessages;

    [UsedImplicitly]
    public class SayHelloApp2Saga : Saga<SayHelloSagaInApp2State>,
                                    IStartThisSagaWhenReceive<SayHelloCommand>,
                                    IHandle<SayGoodByeCommand>
    {
        public ILog Log { get; set; }

        public void Handle(SayGoodByeCommand command)
        {
            Log.Debug("05 \t A friend said good bye, said see you");
            Bus.Reply(new SeeYouCommand("See you"));
            
            CloseSaga();
            Log.Debug("05 \t Closed saga");
        }

        public void Handle(SayHelloCommand command)
        {
            Log.Debug("03 \t A friend said hello");

            Bus.Reply(new SayHelloCommand("Hello back to you!!"));
        }
    }

    public class SayHelloSagaInApp2State : ISagaState
    {
    }
}