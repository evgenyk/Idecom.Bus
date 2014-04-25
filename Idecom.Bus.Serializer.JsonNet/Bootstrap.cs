using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;

namespace Idecom.Bus.Serializer.JsonNet
{
    public static class Bootstrap
    {
        public static Configure JsonNetSerializer(this Configure configure)
        {
            configure.Container.Configure<JsonNetMessageSerializer>(ComponentLifecycle.Singleton);
            return configure;
        }
    }
}