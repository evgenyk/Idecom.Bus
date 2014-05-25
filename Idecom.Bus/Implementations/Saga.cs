﻿using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.Stories;

namespace Idecom.Bus.Implementations
{
    public abstract class Saga<TSagaState> : ISaga<TSagaState> where TSagaState : ISagaState
    {
        protected Saga()
        {
            IsClosed = false;
        }

        public IBus Bus { get; set; }
        public TSagaState Data { get; set; }

        public bool IsClosed { get; private set; }

        public void CloseSaga()
        {
            IsClosed = true;
        }
    }
}