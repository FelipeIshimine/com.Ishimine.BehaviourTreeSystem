using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourTreeSystem.Runtime.Core;
using UnityEngine;

namespace TheKiwiCoder {
    public abstract class CompositeNode : Node {
        [HideInInspector] public List<Node> children = new List<Node>();

        public override Node Clone() {
            CompositeNode node = Instantiate(this);
            node.children = children.ConvertAll(c => c.Clone());
            return node;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            children.RemoveAll(x => x == null);
        }
    }
}