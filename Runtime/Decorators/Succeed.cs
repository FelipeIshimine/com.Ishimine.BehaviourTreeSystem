using System.Collections;
using System.Collections.Generic;
using BehaviourTreeSystem.Runtime.Core;
using UnityEngine;

public class Succeed : DecoratorNode
{
    public override string NodeDescription => "Always returns SUCCESS or RUNNING to the parent";

    protected override void Initialize() { }

    protected override void OnStart() { }

    protected override void OnStop() { }

    protected override State Execution()
    {
        var childResult = child.Execute();
        return childResult == State.Failure ? State.Success : childResult;
    }
}
