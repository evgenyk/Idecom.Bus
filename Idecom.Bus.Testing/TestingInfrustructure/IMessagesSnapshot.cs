namespace Idecom.Bus.Testing.TestingInfrustructure
{
    public interface IMessagesSnapshot
    {
        void HasBeenHandled<TMessage, THandler>(int numberOfTimes = 1);
        void Push(IMessageWithTelemetry messageInfo);
    }
}