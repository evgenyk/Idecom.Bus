namespace Idecom.Bus.Implementations
{
    using System;
    using Annotations;

    [AttributeUsage(AttributeTargets.Class), MeansImplicitUse]
    public class SingleInstanceSagaAttribute : Attribute
    {
    }
}