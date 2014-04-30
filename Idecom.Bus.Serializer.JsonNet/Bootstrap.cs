namespace Idecom.Bus.Serializer.JsonNet
{
    using Implementations;
    using Interfaces;

    public static class Bootstrap
    {
        public static Configure JsonNetSerializer(this Configure configure)
        {
            configure.Container.Configure<JsonNetMessageSerializer>(ComponentLifecycle.Singleton);
            return configure;
        }
    }
}