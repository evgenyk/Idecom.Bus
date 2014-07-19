namespace Idecom.Bus.Tests.Sagas.TwoSagas.FirstSaga
{
    using Implementations;
    using Interfaces;
    using Messages;

    public class Saga1 : Saga<Saga1State>, IStartThisSagaWhenReceive<IStartFirstSagaEvent>
    {
        public static bool Started;

        public void Handle(IStartFirstSagaEvent command)
        {
            Started = true;
            Bus.Raise<IStartSecondSagaEvent>();
        }
    }
}