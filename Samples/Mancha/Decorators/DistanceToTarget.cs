using System;
using BehaviourTreeSystem.Runtime.Core;
using UnityEngine;

public class DistanceToTarget : ConditionalNode
{
    public float distance = 4;

    public Mode mode;
    public enum Mode
    {
        Close,
        Far
    }

    public bool includeEqual = true;
    
    private IGetNavAgentTarget _getNavAgentTarget;
    private Transform _transform;

    public override string GetName() => $"{base.GetName()} \n {GetNodeDistance()}{(mode == Mode.Close?'<':'>')}{(includeEqual?"=":string.Empty)}{distance}";

    public override string OverrideName
    {
        get
        {
            if (string.IsNullOrEmpty(overrideName)) return string.Empty; //Vacio
            return overrideName.Contains("{0}") ? string.Format(overrideName, GetNodeDistance()) : overrideName; //Si tiene formato le agregamos
        }
    }

    private string GetNodeDistance() => Application.isPlaying && Initialized ? $"{GetDistance():F}/{distance:F}" : $"{distance:F}";

    protected override void Initialize()
    {
        GetComponent(out _transform);
        GetComponent(out _getNavAgentTarget);
    }

    protected override void OnStart() { }

    protected override void OnStop() { }

    protected override State Condition()=> GetDistance() <= distance?State.Success:State.Failure;
    private float GetDistance()
    {
        return _getNavAgentTarget.Get() != null?Vector3.Distance(_transform.position, _getNavAgentTarget.Get().position):-1;
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        if(!started || !Application.isPlaying || _getNavAgentTarget == null || _getNavAgentTarget.Get() == null) return;
        
        if (Condition() == State.Success)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_transform.position,_getNavAgentTarget.Get().position);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_transform.position,_getNavAgentTarget.Get().position);
        }
    }
}