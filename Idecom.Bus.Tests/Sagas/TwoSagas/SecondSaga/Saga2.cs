using Xunit;

namespace Idecom.Bus.Tests.Sagas.TwoSagas.SecondSaga
{
    using System.Linq;
    using Implementations;
    using Interfaces;
    using Messages;
    using Xunit.Should;

    public class Saga2 : Saga<Saga2State>, IStartThisSagaWhenReceive<IStartSecondSagaEvent>
    {
        public static bool Started;

        public void Handle(IStartSecondSagaEvent command)
        {
            Assert.NotNull(Data);
            Started = true;
            Bus.Raise<IRsumeFirstSagaAsEventFromSecondSaga>();
        }
    }
}