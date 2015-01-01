namespace Idecom.Bus.Implementations.Telemetry.Snaps
{
    public class BehaviorInvocation : TelemetrySnapBase
    {
        public object Behavior { get; set; }

        public BehaviorInvocation(object behavior)
        {
            Behavior = behavior;
        }
    }
}