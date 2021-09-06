using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using UnityEngine.AI;

public class MoveToPosition : ActionNode
{
    public float speed = 5;
    public float stoppingDistance = 0.1f;
    public bool updateRotation = true;
    public float acceleration = 40.0f;
    public float tolerance = 1.0f;

    private NavMeshAgent _navMeshAgent;

    protected override void Initialize()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    protected override void OnStart() 
    {
        _navMeshAgent.stoppingDistance = stoppingDistance;
        _navMeshAgent.speed = speed;
        _navMeshAgent.destination = blackboard.moveToPosition;
        _navMeshAgent.updateRotation = updateRotation;
        _navMeshAgent.acceleration = acceleration;
    }

    protected override void OnStop() {
    }

    protected override State Execution() {
        if (_navMeshAgent.pathPending) {
            return State.Running;
        }

        if (_navMeshAgent.remainingDistance < tolerance) {
            return State.Success;
        }

        if (_navMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid) {
            return State.Failure;
        }

        return State.Running;
    }
}
