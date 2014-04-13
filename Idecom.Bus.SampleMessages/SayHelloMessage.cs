namespace Idecom.Bus.SampleMessages
{
    public interface IMetAFriendEvent
    {
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
}