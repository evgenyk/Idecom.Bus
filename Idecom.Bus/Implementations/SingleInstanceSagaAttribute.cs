using System;

namespace Idecom.Bus.Implementations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SingleInstanceSagaAttribute : Attribute
    {
        
    }
}