namespace Idecom.Bus.SampleMessages
{
    public interface IMetAFriendEvent
    {
        string Name { get; set; }
    }


    public class SayHelloMessage
    {
        public SayHelloMessage(string hello)
        {
            Hello = hello;
        }

        protected SayHelloMessage()
        {
        }

        public string Hello { get; set; }
    }

    public class SayGoodByeMessage : SayHelloMessage
    {
        public SayGoodByeMessage(string hello) : base(hello)
        {
        }
    }
}