using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    public class Sequencer : CompositeNode 
    {
        protected int Current;

        public override string NodeDescription => "Executes children one at a time left to right. Advances every time a child returns SUCCESS. \nWhen all children return success.\nWhen one child returns failure";

        protected override void Initialize() { }

        protected override void OnStart() 
        {
            Current = 0;
        }

        protected override void OnStop() { }

        protected override State Execution() 
        {
            for (int i = Current; i < children.Count; ++i) {
                Current = i;
                var child = children[Current];

                switch (child.Execute()) {
                    case State.Running:
                        return State.Running;
                    case State.Failure:
                        return State.Failure;
                    case State.Success:
                        continue;
                }
            }
            return State.Success;
        }
    }
}