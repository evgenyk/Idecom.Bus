namespace Idecom.Bus.Interfaces
{
    using Logging;

    public interface ILogFactory
    {
        ILog GetLogger(string name);
    }
}