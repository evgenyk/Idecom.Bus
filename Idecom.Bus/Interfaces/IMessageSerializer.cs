namespace Idecom.Bus.Interfaces
{
    using System;

    public interface IMessageSerializer
    {
        string Serialize(object message);
        object DeSerialize(string message, Type type);
    }
}