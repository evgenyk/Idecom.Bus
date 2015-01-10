namespace Idecom.Bus.Testing.TestingInfrustructure
{
    using Interfaces.Behaviors;

    public class BehaviorInvocation : IBehaviorInvocation
    {
        public BehaviorInvocation(IBehavior behavior)
        {
            Behavior = behavior;
        }

        public object Behavior { get; private set; }
    }
}