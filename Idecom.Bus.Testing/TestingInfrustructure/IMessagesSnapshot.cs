namespace Idecom.Bus.Testing.TestingInfrustructure
{
    public interface IMessagesSnapshot
    {
        bool HasBeenHandled<T>(int numberOfTimes = 1);
        void Push(IMessageWithTelemetry messageInfo);
    }
}