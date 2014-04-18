using System;
using Idecom.Bus.Interfaces;
using Idecom.Bus.SampleMessages;

namespace Idecom.Bus.SampleApp2.Handlers
{
    public class SayHelloHandler: IHandleMessage<SayHelloMessage>, IHandleMessage<SayGoodByeMessage>
    {
        public IBus Bus { get; set; }
        public void Handle(SayHelloMessage message)
        {
            Console.WriteLine("SayHelloMessage");

            Bus.Reply(new SayHelloMessage("Hello back to you!!"));
        }

        public void Handle(SayGoodByeMessage message)
        {
            Console.WriteLine("SayGoodByeMessage");
        }
    }
}