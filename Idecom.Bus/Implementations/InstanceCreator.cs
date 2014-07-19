using System;
using System.Diagnostics;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Utility;

namespace Idecom.Bus.Implementations
{
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