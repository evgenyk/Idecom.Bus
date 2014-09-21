namespace Idecom.Bus.IoC.CastleWindsor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using Castle.Core;
    using Castle.MicroKernel.Lifestyle;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Interfaces;
    using Utility;

    [DebuggerStepThrough]
    public class WindsorContainerAdapter : IContainer
    {
        readonly IWindsorContainer _container;
        Func<IDisposable> _beginScopeFunction;

        public WindsorContainerAdapter(IWindsorContainer container = null)
        {
            _container = container ?? new WindsorContainer();
            _beginScopeFunction = () => _container.BeginScope();
        }

        public void Configure(Type component, ComponentLifecycle componentLifecycle)
        {
            var lifestyleTypeFrom = GetLifestyleTypeFrom(componentLifecycle);
            var services = component.GetInterfaces().Where(x => !x.FullName.StartsWith("System.")).Concat(new[] {component});
            _container.Register(Component.For(services).ImplementedBy(component).LifeStyle.Is(lifestyleTypeFrom).NamedAutomatically(Guid.NewGuid().ToString()));
        }

        public void Configure<T>(ComponentLifecycle componentLifecycle)
        {
            Configure(typeof (T), componentLifecycle);
        }

        public void ConfigureInstance<T>(T instance)
        {
            var component = typeof (T);

            var registration = _container.Kernel.GetAssignableHandlers(component).Select(x => x.ComponentModel).SingleOrDefault();
            if (registration != null)
                throw new Exception("Can not configure a component that has already been registered.");

            var services = component.GetInterfaces().Where(x => !x.FullName.StartsWith("System.")).Concat(new[] {component});
            _container.Register(Component.For(services).Instance(instance).LifeStyle.Is(LifestyleType.Singleton));
        }

        public object Resolve(Type typeToBuild)
        {
            try { return _container.Resolve(typeToBuild); }
            catch (Exception) {
                return null;
            }
        }

        public T Resolve<T>()
        {
            try { return _container.Resolve<T>(); }
            catch (Exception) {
                return default(T);
            }
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return _container.ResolveAll<T>();
        }

        public void Release(object instance)
        {
            _container.Release(instance);
        }

        public IContainer CreateChildContainer()
        {
            var childContainer = new WindsorContainer();
            _container.AddChildContainer(childContainer);
            return new WindsorContainerAdapter(childContainer) {ParentContainer = this};
        }

        public IContainer ParentContainer { get; private set; }

        public void ConfigureUnitOfWork(Func<IDisposable> beginUnitOfWorkFunc = null)
        {
            if (beginUnitOfWorkFunc != null) _beginScopeFunction = beginUnitOfWorkFunc;
        }

        public IDisposable BeginUnitOfWork()
        {
            return _beginScopeFunction();
        }


        public void ConfigureProperty<T>(Expression<Func<T, object>> property, object value)
        {
            var prop = Reflect<T>.GetProperty(property);
            ConfigureProperty<T>(prop.Name, value);
        }

        void ConfigureProperty<T>(string property, object value)
        {
            var component = typeof (T);
            var registration = _container.Kernel.GetAssignableHandlers(component).Select(x => x.ComponentModel).SingleOrDefault();

            if (registration == null)
                throw new InvalidOperationException("Cannot configure property for a type which hadn't been configured yet. Please call 'Configure' first.");

            var propertyInfo = component.GetProperty(property);

            registration.AddProperty(
                new PropertySet(propertyInfo,
                    new DependencyModel(property, value.GetType(), false, true, value)));
        }

        LifestyleType GetLifestyleTypeFrom(ComponentLifecycle componentLifecycle)
        {
            switch (componentLifecycle)
            {
                case ComponentLifecycle.Singleton:
                    return LifestyleType.Singleton;
                case ComponentLifecycle.PerUnitOfWork:
                    return LifestyleType.Scoped;
                case ComponentLifecycle.Transient:
                    return LifestyleType.Transient;
                default:
                    throw new ArgumentOutOfRangeException("componentLifecycle", string.Format("Unknown lifestype {0}, please check what's going on", componentLifecycle));
            }
        }
    }
}