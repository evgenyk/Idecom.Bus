namespace Idecom.Bus.Implementations.Behaviors
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Interfaces.Behaviors;

    public class BehaviorChain : IBehaviorChain
    {
        readonly List<Type> _chain = new List<Type>();

        public IEnumerator<Type> GetEnumerator()
        {
            return _chain.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public BehaviorChain WrapWith<T>() where T : IBehavior
        {
            _chain.Add(typeof (T));
            return this;
        }

        public BehaviorChain AndThenWith<T>() where T : IBehavior
        {
            
            return WrapWith<T>();
        }

        public BehaviorChain AndThis<T>() where T : IBehavior
        {
            return WrapWith<T>();
        }
    }
}