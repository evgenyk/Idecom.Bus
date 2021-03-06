using Xunit;

namespace Idecom.Bus.Tests.Sagas.TwoSagas.FirstSaga
{
    using System.Linq;
    using Implementations;
    using Interfaces;
    using Messages;
    using Xunit.Should;

    public class Saga1 : Saga<Saga1State>, IStartThisSagaWhenReceive<IStartFirstSagaEvent>, IHandle<IRsumeFirstSagaAsEventFromSecondSaga>
    {
        public static bool Started;

        public void Handle(IStartFirstSagaEvent command)
        {
            Assert.NotNull(Data);
            Started = true;
            Bus.Publish<IStartSecondSagaEvent>();
        }

        public void Handle(IRsumeFirstSagaAsEventFromSecondSaga command)
        {
            Bus.IncomingMessageContext.IncomingHeaders.Count().ShouldBe(2);
            CloseSaga();
        }
    }
}