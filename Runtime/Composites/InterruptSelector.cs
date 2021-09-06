namespace BehaviourTreeSystem.Runtime {
    public class InterruptSelector : Selector 
    {
        public override string NodeDescription => "Executes children one at a time Left to Right. Advance when a child returns FAILURE. Children are constantly reevaluated each tick, if a child with higher priority changes state from FAILURE, the current running child is aborted \nSUCCESS:When one child returns success.\nFAILURE:When all children return FAILURE";
        
        protected override State Execution() 
        {
            int previous = Current-1;
            base.OnStart();
            var status = base.Execution();
            
            if (previous >= 0 && previous != Current) 
            {
                if (children[previous].state != State.Failure) 
                    children[Current].Abort();
            }

            return status;
        }
    }
}