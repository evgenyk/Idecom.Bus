namespace Idecom.Bus.Implementations
{
    using Interfaces;

    public abstract class NestedConfigurationBase
    {
        readonly Configure _rootConfiguration;

        protected NestedConfigurationBase(Configure rootConfiguration)
        {
            _rootConfiguration = rootConfiguration;
        }

        public Configure RootConfiguration
        {
            get { return _rootConfiguration; }
        }

        public IContainer Container
        {
            get { return _rootConfiguration.Container; }
            set { _rootConfiguration.Container = value; }
        }
    }
}