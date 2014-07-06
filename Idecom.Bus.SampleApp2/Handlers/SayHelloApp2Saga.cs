using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces.Addons.Sagas;

namespace Idecom.Bus.SampleApp2.Handlers
{
    using System;
    using Interfaces;
    using SampleMessages;

    public class SayHelloApp2Saga : Saga<SayHelloSagaInApp2State>, 
        IStartThisSagaWhenReceive<SayHelloCommand>, 
        IHandle<SayGoodByeCommand>
    {
        public void Handle(SayGoodByeCommand command)
        {
            Console.WriteLine("A friend said good bye, said see you");
            Bus.Reply(new SeeYouCommand("See you"));
        }

        public void Handle(SayHelloCommand command)
        {
            Console.WriteLine("A friend said hello");

            Bus.Reply(new SayHelloCommand("Hello back to you!!"));
        }
    }

    public class SayHelloSagaInApp2State : ISagaState
    {
    }
}