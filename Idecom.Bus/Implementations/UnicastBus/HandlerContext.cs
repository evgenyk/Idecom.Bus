namespace Idecom.Bus.Implementations.UnicastBus
{
    using System.Reflection;

    public class HandlerContext
    {
        public MethodInfo Method { get; set; }
    }
}