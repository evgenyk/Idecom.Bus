using System;
using System.Linq.Expressions;

namespace Idecom.Bus.Interfaces
{
    public interface IContainer
    {
        IContainer ParentContainer { get; }
        void Configure(Type component, ComponentLifecycle componentLifecycle);
        void Configure<T>(ComponentLifecycle componentLifecycle);
        void ConfigureInstance<T>(T instance);
        object Resolve(Type typeToBuild);
        T Resolve<T>();
        void Release(object instance);
        IContainer CreateChildContainer();
        void ConfigureUnitOfWork(Func<IDisposable> beginUnitOfWorkFunc = null);
        IDisposable BeginUnitOfWork();
        void ConfigureProperty<T>(Expression<Func<T, object>> property, object value);
    }

    public enum ComponentLifecycle
    {
        Singleton,
        PerUnitOfWork,
        Transient
    }
}