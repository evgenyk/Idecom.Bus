namespace Idecom.Bus.Serializer.JsonNet
{
    using System;
    using System.Runtime.Serialization;
    using Interfaces;

    public class InterfaceSupportingBinder : SerializationBinder
    {
        readonly IInstanceCreator _instanceCreator;

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