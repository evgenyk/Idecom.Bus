namespace Idecom.Bus.Implementations.Telemetry.Snaps
{
    using Interfaces.Behaviors;
    using Interfaces.Telemetry;

    public class BehaviorInvocation : TelemetrySnapBase, IHaveBehavior
    {
        public IBehavior Behavior { get; set; }

        public BehaviorInvocation(IBehavior behavior)
        {
            Behavior = behavior;
        }
    }
}