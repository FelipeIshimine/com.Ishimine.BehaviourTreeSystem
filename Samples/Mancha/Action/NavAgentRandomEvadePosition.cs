using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using UnityEngine.AI;

public class NavAgentRandomEvadePosition : ActionNode
{
    public LayerMask areaMask = ~0;
    private NavMeshAgent _navMeshAgent;
    private Transform _transform;
    private IGetNavAgentTarget _target;
    
    public float angleVariation = 5;
    public float moveRadius = 10;
    public float targetReachedThreshold = 1;

    public float angleVariationOnFail = 5;

    public int maxTries = 10;
    
    protected override void Initialize()
    {
        GetComponent(out _target);
        GetComponent(out _navMeshAgent);
        GetComponent(out _transform);
    }

    protected override void OnStart()
    {
        FindNextDestination();
    }

    protected override void OnStop() 
    {
    }

    protected override State Execution()
    {
        if (!TargetReached()) return State.Running;
        return FindNextDestination() ? State.Success : State.Failure;
    }
    
    private bool FindNextDestination()
    {
        if (!_navMeshAgent.isOnNavMesh)
            return false;
        Transform evadeTarget = _target.Get();
        if (evadeTarget == null)
            return false;

        int tryCount = 0;
        Vector3 myPosition;
        NavMeshHit navMeshHit = default;
        do
        {
            myPosition = GetRandomEvadePosition(evadeTarget, angleVariation + angleVariationOnFail * tryCount);
            tryCount++;
            if(tryCount > maxTries) break;
        } while (!NavMesh.SamplePosition(myPosition, out navMeshHit, moveRadius, areaMask.value) || !navMeshHit.hit);

        if (!navMeshHit.hit) 
            return false;
        _navMeshAgent.destination = navMeshHit.position;
        return true;
    }

    private Vector3 GetRandomEvadePosition(Transform evadeTarget, float angleVariation)
    {
        Vector2 myPosition = new Vector2(_transform.position.x, _transform.position.z);
        Vector2 evadeTargetPosition = new Vector2(evadeTarget.position.x, evadeTarget.position.z);
        Vector2 newDirection = (myPosition - evadeTargetPosition).normalized;
        float angle = AsAngle(newDirection) + Random.Range(-angleVariation, angleVariation);
        newDirection = AsVector(angle);
        return new Vector3(newDirection.x, 0, newDirection.y) * moveRadius + _transform.position;
    }

    private static float AsAngle(Vector2 source)
    {
        Vector2 normalized = source.normalized;
        float angle = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg;
        return angle;
    }
    
    private static Vector2 AsVector(float degree)=> RadianToVector2(degree * Mathf.Deg2Rad);
    
    private static Vector2 RadianToVector2(float radian)=> new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));

    private bool TargetReached()
    {
        float distance = Vector3.Distance(_transform.position, _navMeshAgent.destination);
        return distance < targetReachedThreshold;
    }

}