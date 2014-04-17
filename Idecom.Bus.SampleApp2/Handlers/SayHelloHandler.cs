using Idecom.Bus.Interfaces;

namespace Idecom.Bus.SampleMessages.Handlers
{
    public class SayHelloHandler: IHandleMessage<SayHelloMessage>
    {
        public IBus Bus { get; set; }
        public void Handle(SayHelloMessage message)
        {
            Bus.Reply(new SayHelloMessage("Hello back to you!!"));
        }
    }
}