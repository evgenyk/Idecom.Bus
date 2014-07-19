namespace Idecom.Bus.Tests.Sagas.TwoSagas.SecondSaga
{
    using Implementations;
    using Interfaces;
    using Messages;

    public class Saga2 : Saga<Saga2State>, IStartThisSagaWhenReceive<IStartSecondSagaEvent>
    {
        public static bool Started;

        public void Handle(IStartSecondSagaEvent command)
        {
            Started = true;
        }
    }
}