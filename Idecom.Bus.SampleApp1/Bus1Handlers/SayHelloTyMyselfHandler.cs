namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    using System;
    using Interfaces;
    using SampleMessages;

    public class SayHelloTyMyselfHandler : IHandle<SayHelloCommand>
    {
        public void Handle(SayHelloCommand command)
        {
            Console.WriteLine("Seem like I'm wired talking to myself ");
        }
    }
}