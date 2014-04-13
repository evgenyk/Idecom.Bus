using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.SampleMessages;

namespace Idecom.Bus.SampleApp1.Bus1Handlers
{
    /// <summary>
    /// Saga functionality is still in works, nothing yet implemented. 
    /// Please call your dealer for updates.
    /// Right now having the same messages assigned to more than one handler would break nastily
    /// </summary>
    public class NicePersonConversationSaga : Saga<NicePersonConversationSagaState>,
        IStartThisSagaWhenReceive<IMetAFriendEvent>
//        IHandleMessage<SayHelloMessage>
    {
        public void Handle(IMetAFriendEvent message)
        {
            throw new System.NotImplementedException();
        }
    }

    public class NicePersonConversationSagaState : ISagaState
    {
    }
}
