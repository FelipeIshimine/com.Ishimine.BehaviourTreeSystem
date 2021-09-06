using BehaviourTreeSystem.Runtime.Core;

namespace BehaviourTreeSystem.Runtime {
    public class Failure : DecoratorNode
    {
        public override string NodeDescription => "Always returns FAILURE or RUNNING to the parent";

        protected override void Initialize() { }

        protected override void OnStart() { }

        protected override void OnStop() { }

        protected override State Execution() {
            var childResult = child.Execute();
            return childResult == State.Success ? State.Failure : childResult;
        }
    }
}