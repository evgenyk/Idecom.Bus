namespace Idecom.Bus.SampleMessages
{
    public class SayHelloCommand
    {
        public SayHelloCommand(string hello)
        {
            Hello = hello;
        }

        protected SayHelloCommand()
        {
        }

        public string Hello { get; set; }
    }

    public class SayGoodByeCommand : SayHelloCommand
    {
        public SayGoodByeCommand(string hello) : base(hello)
        {
        }
    }
}