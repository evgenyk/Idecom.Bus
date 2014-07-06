using Idecom.Bus.Interfaces.Addons.Sagas;

namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    public class NicePersonConversationState : ISagaState
    {
        public bool FriendSaidHello { get; set; }
        public bool RepliedBackWithHello { get; set; }
        public bool Started { get; set; }
    }
}