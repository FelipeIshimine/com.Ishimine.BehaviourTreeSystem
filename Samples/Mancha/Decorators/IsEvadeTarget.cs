using System;
using BehaviourTreeSystem;
using BehaviourTreeSystem.Runtime.Core;
using TheKiwiCoder;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class IsEvadeTarget : ConditionalNode
{
    private IGetNavAgentTarget _getNavAgentTarget;
    private Transform _transform;

    protected override void Initialize()
    {
        GetComponent(out _transform);
        GetComponent(out _getNavAgentTarget);
    }

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }
    
    protected override State Condition() => _getNavAgentTarget.Get() == _transform?State.Success:State.Failure;
}