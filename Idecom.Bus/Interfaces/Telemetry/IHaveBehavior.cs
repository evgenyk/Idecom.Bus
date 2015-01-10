namespace Idecom.Bus.Interfaces.Telemetry
{
    using Behaviors;

    public interface IHaveBehavior
    {
        IBehavior Behavior { get; }
    }
}