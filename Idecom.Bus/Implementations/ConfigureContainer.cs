namespace Idecom.Bus.Implementations
{
    public class ConfigureContainer : NestedConfigurationBase
    {
        public ConfigureContainer(Configure rootConfiguration) : base(rootConfiguration)
        {
        }
    }
    public class ConfigureLogging : NestedConfigurationBase
    {
        public ConfigureLogging(Configure rootConfiguration) : base(rootConfiguration)
        {
        }
    }
}