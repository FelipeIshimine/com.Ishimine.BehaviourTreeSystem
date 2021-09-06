using TheKiwiCoder;

namespace BehaviourTreeSystem.Runtime {
    public class Selector : CompositeNode {
        protected int Current;

        public override string NodeDescription => "Executes children one at a time Left to Right. Advance when a child returns FAILURE.\nSUCCESS:When one child returns success.\nFAILURE:When all children return FAILURE";

        protected override void Initialize()
        {
        }

        protected override void OnStart() {
            Current = 0;
        }

        protected override void OnStop() {
        }

        protected override State Execution() {
            for (int i = Current; i < children.Count; ++i) {
                Current = i;
                var child = children[Current];

                switch (child.Execute()) 
                {
                    case State.Running:
                        return State.Running;
                    case State.Success:
                        return State.Success;
                    case State.Failure:
                        continue;
                }
            }
            return State.Failure;
        }
    }
}