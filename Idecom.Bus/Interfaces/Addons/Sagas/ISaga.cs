﻿namespace Idecom.Bus.Interfaces.Addons.Sagas
{
    public interface ISaga
    {
        bool IsClosed { get; }
    }

    public interface ISaga<out TSagaState> : ISaga where TSagaState : ISagaState
    {
        TSagaState Data { get; }
    }
}