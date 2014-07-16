using System.Diagnostics;

namespace Idecom.Bus.Implementations
{
    using System;
    using Interfaces;
    using Utility;

    [DebuggerStepThrough]
    public class InstanceCreator : IInstanceCreator
    {
        public object CreateInstanceOf(Type type)
        {
            var newType = type;

            if (type.IsInterface)
                newType = InterfaceImplementor.ImplementInterface(type);

            var result = Activator.CreateInstance(newType);
            return result;
        }

        public T CreateInstanceOf<T>()
        {
            return (T) CreateInstanceOf(typeof (T));
        }
    }
}