using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Tests.Sagas.TwoSagas.Messages;

namespace Idecom.Bus.Tests.Sagas.TwoSagas.SecondSaga
{
    public class Saga2 : Saga<Saga2State>, IStartThisSagaWhenReceive<IStartSecondSagaEvent>
    {
        public static bool Started;

        public void Handle(IStartSecondSagaEvent command)
        {
            Started = true;
        }
    }
}