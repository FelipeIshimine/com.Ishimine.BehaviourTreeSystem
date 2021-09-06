using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;

public class SetClosestChaseTargetAsEvadeTarget : ActionNode
{
    private IGetNavAgentTargets _getNavAgentTargets;
    private Transform _transform;
    protected override void Initialize()
    {
        GetComponent(out _transform);
        GetComponent(out _getNavAgentTargets);
    }

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State Execution()
    {
        var closest = GetClosest(_getNavAgentTargets.Get());
        closest.gameObject.GetComponent<ISetAsEvadeTarget>().Set(true);
        return State.Success;
    }
    
    private Transform GetClosest(IEnumerable<Transform> getAgents)
    {
        Transform transform = _transform;
        float sqrDistance = float.MaxValue;
        Transform closest = null;
        foreach (Transform agent in getAgents)
        {
            float currentSqrDistance = (transform.position - agent.position).sqrMagnitude;
            if (currentSqrDistance < sqrDistance)
            {
                sqrDistance = currentSqrDistance;
                closest = agent.transform;
            }
        }
        return closest;
    }
}