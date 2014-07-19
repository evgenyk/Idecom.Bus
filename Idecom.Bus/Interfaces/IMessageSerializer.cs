using System;

namespace Idecom.Bus.Interfaces
{
    public interface IMessageSerializer
    {
        string Serialize(object message);
        object DeSerialize(string message, Type type);
    }
}