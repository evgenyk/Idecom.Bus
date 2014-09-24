namespace Idecom.Bus.Interfaces
{
    public interface IMessageContext
    {
        int Attempt { get; }
        int MaxAttempts { get; }
    }
}