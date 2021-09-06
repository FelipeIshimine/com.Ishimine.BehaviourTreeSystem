using System.Collections;
using System.Collections.Generic;
using BehaviourTreeSystem.Runtime.Core;
using UnityEngine;

namespace TheKiwiCoder {

    public class RootNode : Node {
        public Node child;

        protected override void Initialize()
        {
        }
        
        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State Execution() {
            return child.Execute();
        }

        public override Node Clone() {
            RootNode node = Instantiate(this);
            node.child = child.Clone();
            return node;
        }
    }
}