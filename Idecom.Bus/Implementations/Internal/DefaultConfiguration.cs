namespace Idecom.Bus.Implementations.Internal
{
    using System;

    internal class DefaultConfiguration
    {
        public static Func<Type, bool> DefaultEventNamingConvention
        {
            get { return type => type.IsInterface && type.Name.EndsWith("event", StringComparison.CurrentCultureIgnoreCase); }
        }

        public static Func<Type, bool> DefaultCommandNamingConvention
        {
            get { return type => !type.IsInterface && type.Name.EndsWith("command", StringComparison.CurrentCultureIgnoreCase); }
        }

        public static Func<Type, bool> DefaultHandlerConvention
        {
            get { return type => true; }
        }
    }
}