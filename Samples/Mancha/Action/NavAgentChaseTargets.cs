using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;
using UnityEngine.AI;

public class NavAgentChaseTargets : ActionNode
{
    public float reachedDistance = 1.1f;

    [Tooltip("La busqueda se da por concluida apenas se encuentre alguien dentro del 'reached distance' incluso si no es el mas cercano")]
    public bool useShortCircuit = true;  
    private NavMeshAgent _navMeshAgent;
    private IGetNavAgentTargets _iGetNavAgentTargets;

    public override string GetName() => $"{base.GetName()} \n Reach:{reachedDistance}";

    protected override void Initialize()
    {
        GetComponent(out _navMeshAgent);
        GetComponent(out _iGetNavAgentTargets);
    }

    protected override void OnStart()
    {
        
    }

    protected override void OnStop() { }

    protected override State Execution()
    {
        Transform closest = GetClosest(_iGetNavAgentTargets.Get());

        if (closest == null)
        {
            Debug.Log("Closest is NULL");
            return State.Failure;
        }

        float distance = Vector3.Distance(closest.transform.position, _navMeshAgent.transform.position);
        _navMeshAgent.destination = closest.position;
        if (distance < reachedDistance)
        {
            Debug.Log($"closest is :{closest.name}");
            Debug.Log(distance);
            return State.Success;
        }
        
        Debug.Log($"Chasing:{closest.gameObject.name}");

        return State.Running;
    }

    private Transform GetClosest(IEnumerable<Transform> getAgents)
    {
        Transform transform = _navMeshAgent.transform;
        float sqrDistance = float.MaxValue;
        Transform closest = null;
        foreach (Transform agent in getAgents)
        {
            if (agent == transform) continue;
            float currentSqrDistance = (transform.position - agent.position).sqrMagnitude;
            if (currentSqrDistance < sqrDistance)
            {
                sqrDistance = currentSqrDistance;
                closest = agent.transform;
                if (useShortCircuit && sqrDistance < reachedDistance)
                    break;
            }
        }
        return closest;
    }
    
    
  
}