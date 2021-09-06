using BehaviourTreeSystem.Runtime.Core;

namespace BehaviourTreeSystem.Runtime.Decorators {
    public class Repeat : DecoratorNode 
    {
        public bool restartOnSuccess = true;
        public bool restartOnFailure = true;

        public override string NodeDescription => $"If it's child returns {GetSuccessOrFailureText()}, it will try again and return RUNNING to it's parent";

        public override string GetName() => $"{base.GetName()} on:\n{GetSuccessOrFailureText()}";

        private string GetSuccessOrFailureText()
        {
            if (restartOnFailure && restartOnSuccess)
                return "Success or Failure";

            if (restartOnFailure)
                return "Failure";

            if (restartOnSuccess)
                return "Success";

            return $"{base.GetName()} on:\n Never";
        }

        protected override void Initialize() { }

        protected override void OnStart() { }

        protected override void OnStop() { }

        protected override State Execution() {
            switch (child.Execute()) {
                case State.Running:
                    break;
                case State.Failure:
                    return restartOnFailure ? State.Running : State.Failure;
                case State.Success:
                    return restartOnSuccess ? State.Running : State.Success;
            }
            return State.Running;
        }
    }
}
