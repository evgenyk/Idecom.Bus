namespace Idecom.Bus.Interfaces
{
    using System.Collections.Generic;

    public interface IMessageContext
    {
        int Attempt { get; }
        int MaxAttempts { get; }
        IEnumerable<KeyValuePair<string, string>> IncomingHeaders { get; }
    }
}