namespace Idecom.Bus.Interfaces
{
    public interface IBusInstance : IBus
    {
        bool IsStarted { get; }
        IBusInstance Start();
        void Stop();
    }
}