namespace Idecom.Bus.Interfaces
{
    using System;

    public interface IInstanceCreator
    {
        object CreateInstanceOf(Type type);
        T CreateInstanceOf<T>();
    }
}