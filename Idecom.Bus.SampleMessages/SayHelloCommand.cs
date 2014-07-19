namespace Idecom.Bus.SampleMessages
{
    using System;

    public class SayHelloCommand
    {
        public SayHelloCommand(string greeting)
        {
            Greeting = greeting;
            InterestingWebsite = new Uri("http://www.whatever.com");
            HaventSeenYouSince = TimeSpan.FromDays(35);
        }

        protected SayHelloCommand()
        {
        }

        public string Greeting { get; set; }
        public Uri InterestingWebsite { get; set; }
        public TimeSpan HaventSeenYouSince { get; set; }
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