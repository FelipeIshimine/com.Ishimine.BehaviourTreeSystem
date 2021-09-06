using System.Collections.Generic;
using System.Linq;
using TheKiwiCoder;

namespace BehaviourTreeSystem.Runtime {
    public class Parallel : CompositeNode
    {
        public bool abortOnAnyFail = false;
        private readonly List<State> _childrenLeftToExecute = new List<State>();

        public override string NodeDescription =>
            $"Executes all children 'at once' concurrently. Multiple children can be in the running state at the same time\nSUCCESS:When all children return success FAILURE:When one child returns failure (if {nameof(abortOnAnyFail)} is TRUE), Remaining children are aborted.";


        protected override void Initialize()
        {
        }

        protected override void OnStart() {
            _childrenLeftToExecute.Clear();
            children.ForEach(a => {
                _childrenLeftToExecute.Add(State.Running);
            });
        }

        protected override void OnStop() {
        }

        protected override State Execution() {
            bool stillRunning = false;
            for (int i = 0; i < _childrenLeftToExecute.Count(); ++i) 
            {
                if (_childrenLeftToExecute[i] == State.Running) {
                    var status = children[i].Execute();
                    if (status == State.Failure && abortOnAnyFail) {
                        AbortRunningChildren();
                        return State.Failure;
                    }

                    if (status != State.Success) 
                        stillRunning = true;

                    _childrenLeftToExecute[i] = status;
                }
            }

            return stillRunning ? State.Running : State.Success;
        }

        void AbortRunningChildren() {
            for (int i = 0; i < _childrenLeftToExecute.Count(); ++i) {
                if (_childrenLeftToExecute[i] == State.Running) {
                    children[i].Abort();
                }
            }
        }
    }
}