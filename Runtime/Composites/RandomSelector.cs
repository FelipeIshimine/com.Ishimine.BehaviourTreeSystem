using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TheKiwiCoder {
    public class RandomSelector : CompositeNode {
        
        protected int Current;

        public override string NodeDescription => "Randomly selects one child to execute and returns it result";

        protected override void Initialize()
        {
        }

        protected override void OnStart() {
            Current = Random.Range(0, children.Count);
        }

        protected override void OnStop() {
        }

        protected override State Execution() {
            var child = children[Current];
            return child.Execute();
        }
    }
}