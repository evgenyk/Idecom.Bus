namespace Idecom.Bus.SampleApp2.Handlers
{
    using System;
    using Interfaces;
    using SampleMessages;

    public class SayHelloHandler : IHandle<SayHelloCommand>, IHandle<SayGoodByeCommand>
    {
        public IBus Bus { get; set; }

        public void Handle(SayGoodByeCommand command)
        {
            Console.WriteLine("SayGoodByeCommand");
        }

        public void Handle(SayHelloCommand command)
        {
            Console.WriteLine("SayHelloCommand");

            Bus.Reply(new SayHelloCommand("Hello back to you!!"));
        }
    }
}