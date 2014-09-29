namespace Idecom.Bus.Testing.TestingInfrustructure
{
    using System.Collections.Generic;
    using Implementations.UnicastBus;

    public class TestBus: Bus
    {
        public TestBus()
        {
            MessagesReceived = new List<object>();
        }

        public new TestBus Start()
        {
            base.Start();
            return this; //returning TestBus instead to expose more stuff for testing purposes
        }

        public IList<object> MessagesReceived { get; private set; }
    }
}