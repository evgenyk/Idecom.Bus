namespace Idecom.Bus.Testing.TestingInfrustructure
{
    using Addressing;
    using Annotations;
    using Implementations.UnicastBus;
    using Interfaces;

    public class TestBus: Bus
    {
        public TestBus(Address localAddress, ILogFactory logFactory) : base(localAddress, logFactory)
        {
            Snapshot = new MessagesSnapshot();
        }

        public new TestBus Start()
        {
            base.Start();
            return this; //returning TestBus instead to expose more stuff for testing purposes
        }

        public IMessagesSnapshot Snapshot { get; private set; }
    }
}