using UnityEngine;

namespace BehaviourTreeSystem.Runtime.Core
{
    /// <summary>
    /// It has a #Condition():State and an optional -Child:Node.
    /// If the -Child:Node is assigned and the #Condition():State is Success. Returns the Child.Execution() result. Otherwise returns the Condition():State result
    /// </summary>
    public abstract class ConditionalNode : Node
    {
        [HideInInspector,SerializeField] private Node child;

        public bool revert;

        protected abstract State Condition();

        protected virtual bool IsReady() => true;

        public Node Child
        {
            get => child;
            set => child = value;
        }

        protected override State Execution()
        {
            if (!IsReady()) return State.Failure;
            
            var value = Condition();

            if (revert && value != State.Running)
                value =  (State)(-(int)value);

            if (value == State.Success && child) return child.Execute();
            
            if(value == State.Failure && child && child.state == State.Running)
                child.Abort();
            
            return value;
        }
        
        public override Node Clone() 
        {
            ConditionalNode node = Instantiate(this);
            if(node.child) node.child = child.Clone();
            return node;
        }
    }
}