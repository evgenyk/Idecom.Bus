namespace Idecom.Bus.Interfaces
{
    public interface IBusInstance : IBus
    {
        IBusInstance Start();
        void Stop();
    }
}