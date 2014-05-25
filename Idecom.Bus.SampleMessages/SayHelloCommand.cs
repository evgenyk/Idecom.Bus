namespace Idecom.Bus.SampleMessages
{
    public class SayHelloCommand
    {
        public SayHelloCommand(string greeting)
        {
            Greeting = greeting;
        }

        protected SayHelloCommand()
        {
        }

        public string Greeting { get; set; }
    }

    public class SayGoodByeCommand : SayHelloCommand
    {
        public SayGoodByeCommand(string greeting) : base(greeting)
        {
        }
    }

    public class SeeYouCommand : SayHelloCommand
    {
        public SeeYouCommand(string greeting) : base(greeting)
        {
        }
    }
}