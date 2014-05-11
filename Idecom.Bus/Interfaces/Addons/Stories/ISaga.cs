using Idecom.Bus.Utility;

namespace Idecom.Bus.Interfaces.Addons.Stories
{
    using System;

    public interface ISaga<out TSagaState> where TSagaState : ISagaState
    {
        TSagaState Data { get; }
    }

    public abstract class ContainSagaState : ISagaState
    {
        private string _id;

        protected ContainSagaState()
        {
            _id = ShortGuid.NewGuid();
        }
    }
}