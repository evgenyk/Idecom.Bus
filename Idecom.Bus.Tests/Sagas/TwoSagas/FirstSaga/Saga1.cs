using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Tests.Sagas.TwoSagas.Messages;

namespace Idecom.Bus.Tests.Sagas.TwoSagas.FirstSaga
{
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