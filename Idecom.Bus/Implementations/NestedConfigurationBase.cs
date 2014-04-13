using Idecom.Bus.Interfaces;

namespace Idecom.Bus.Implementations
{
    public abstract class NestedConfigurationBase
    {
        protected Configure _rootConfiguration;

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