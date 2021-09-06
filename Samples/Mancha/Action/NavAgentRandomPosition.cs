using TheKiwiCoder;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentRandomPosition : ActionNode
{
    public LayerMask areaMask = ~0;
    private NavMeshAgent _navMeshAgent;
    private Transform _transform;
    
    public float moveRadius = 10;
    public float targetReachedThreshold = 1;
    public int maxTries = 10;
    
    protected override void Initialize()
    {
        GetComponent(out _navMeshAgent);
        GetComponent(out _transform);
    }

    protected override void OnStart()
    {
        FindNextDestination();
    }

    protected override void OnStop() {
    }

    protected override State Execution()
    {
        if (!TargetReached()) return State.Running;
        return FindNextDestination() ? State.Success : State.Failure;
    }
    
    private bool FindNextDestination()
    {
        if (!_navMeshAgent.isOnNavMesh) return false;
        
        int tryCount = 0;
        NavMeshHit navMeshHit = default;
        Vector3 endPosition;
        do
        {
            tryCount++;
            if(tryCount > maxTries) break;
            
            Vector3 randomDirection = Random.insideUnitSphere * moveRadius;
            endPosition = _transform.position + randomDirection;

          
        } while (!NavMesh.SamplePosition(endPosition, out navMeshHit, moveRadius, areaMask.value) ||
                 !navMeshHit.hit);

        if (!navMeshHit.hit) return false;
        _navMeshAgent.destination = navMeshHit.position;
        return true;
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