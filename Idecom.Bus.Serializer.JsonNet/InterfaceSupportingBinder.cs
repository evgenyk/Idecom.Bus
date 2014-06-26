using System;
using System.Runtime.Serialization;
using Idecom.Bus.Interfaces;

namespace Idecom.Bus.Serializer.JsonNet
{
    public class InterfaceSupportingBinder : SerializationBinder
    {
        private readonly IInstanceCreator _instanceCreator;

        public InterfaceSupportingBinder(IInstanceCreator instanceCreator)
        {
            _instanceCreator = instanceCreator;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            return null;
        }
    }
}