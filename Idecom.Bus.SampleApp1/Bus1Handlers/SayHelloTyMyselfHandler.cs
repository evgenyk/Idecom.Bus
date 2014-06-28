using System;
using Castle.MicroKernel;
using Idecom.Bus.Interfaces;
using Idecom.Bus.SampleMessages;

namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    public class SayHelloTyMyselfHandler: IHandle<SayHelloCommand>
    {
        public void Handle(SayHelloCommand command)
        {
            Console.WriteLine("Seem like I'm wired talking to myself ");
        }
    }
}