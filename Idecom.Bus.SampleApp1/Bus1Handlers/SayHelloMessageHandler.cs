using System;
using Idecom.Bus.Interfaces;
using Idecom.Bus.SampleMessages;

namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    public class SayHelloMessageHandler //: IHandleMessage<SayHelloMessage>
    {
        public IBus Bus { get; set; }

        public void Handle(SayHelloMessage message)
        {
            PerfMetric.Received++;
            //if (PerfMetric.Received % 100 == 0)
            Console.WriteLine(PerfMetric.Received + " - Bus1 - Message received : " + message.Hello);
            //Bus.Reply(new SayHelloMessage("Hello back!!!"));
            //throw new Exception("Bjorked!!!!");
        }
    }
}