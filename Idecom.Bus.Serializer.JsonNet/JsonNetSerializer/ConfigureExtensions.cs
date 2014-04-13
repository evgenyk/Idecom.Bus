using Idecom.Bus.Implementations;

namespace Idecom.Bus.Serializer.JsonNet.JsonNetSerializer
{
    public static class ConfigureExtensions
    {
        public static Configure JsonNetSerializer(this Configure configure)
        {
            configure.Container.ConfigureInstance(new JsonNetMessageSerializer());
            return configure;
        }
    }
}