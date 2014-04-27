using System;
using Idecom.Bus.Interfaces;
using Idecom.Bus.SampleMessages;

namespace Idecom.Bus.SampleApp2.Handlers
{
    public class SayHelloHandler: IHandle<SayHelloCommand>, IHandle<SayGoodByeCommand>
    {
        public IBus Bus { get; set; }
        public void Handle(SayHelloCommand command)
        {
            Console.WriteLine("SayHelloCommand");

            Bus.Reply(new SayHelloCommand("Hello back to you!!"));
        }

        public void Handle(SayGoodByeCommand command)
        {
            Console.WriteLine("SayGoodByeCommand");
        }
    }
}