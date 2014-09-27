namespace Idecom.Bus.SampleApp2.Handlers
{
    using System;
    using Annotations;
    using Implementations;
    using Interfaces;
    using Interfaces.Addons.Sagas;
    using SampleMessages;

    [UsedImplicitly]
    public class SayHelloApp2Saga : Saga<SayHelloSagaInApp2State>,
                                    IStartThisSagaWhenReceive<SayHelloCommand>,
                                    IHandle<SayGoodByeCommand>
    {
        public void Handle(SayGoodByeCommand command)
        {
            Console.WriteLine("05 \t A friend said good bye, said see you");
            Bus.Reply(new SeeYouCommand("See you"));
            
            CloseSaga();
            Console.WriteLine("05 \t Closed saga");
        }

        public void Handle(SayHelloCommand command)
        {
            Console.WriteLine("03 \t A friend said hello");

            Bus.Reply(new SayHelloCommand("Hello back to you!!"));
        }
    }

    public class SayHelloSagaInApp2State : ISagaState
    {
    }
}