namespace Idecom.Bus.Implementations
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class SingleInstanceSagaAttribute : Attribute
    {
    }
}