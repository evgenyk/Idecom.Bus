using System;

namespace Idecom.Bus.Interfaces
{
    public interface IInstanceCreator
    {
        object CreateInstanceOf(Type type);
        T CreateInstanceOf<T>();
    }
}