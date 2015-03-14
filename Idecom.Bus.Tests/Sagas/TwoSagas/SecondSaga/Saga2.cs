namespace Idecom.Bus.Tests.Sagas.TwoSagas.SecondSaga
{
    using System;
    using Implementations;
    using Interfaces;
    using Messages;
    using Xunit;

    public class Saga2 : Saga<Saga2State>, IStartThisSagaWhenReceive<IStartSecondSagaEvent>, IHandle<IAmRandomWhichDoesntStartASagaEvent>
    {
        public static bool Started;

        public void Handle(IAmRandomWhichDoesntStartASagaEvent command)
        {
            throw new Exception("Should never be called");
        }

        public void Handle(IStartSecondSagaEvent command)
        {
            Assert.NotNull(Data);
            Started = true;
            Bus.Publish<IRsumeFirstSagaAsEventFromSecondSaga>();
            CloseSaga();
        }
    }
}