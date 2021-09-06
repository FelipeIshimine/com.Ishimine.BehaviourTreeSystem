using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerAgent : MonoBehaviour, IGetNavAgentTargets, IGetNavAgentTarget, ISetAsEvadeTarget
{
    public Renderer render;
    
    public Material normalMaterial;
    public Material monsterMaterial;
    
    public float evadeRadius = 3;
    public float moveRadius = 6;
    public float targetReachedDistanceThreshold = .2f;
    public float angleVariation = 45;
    public float touchDistance = .5f;
    public float waitTime = 1.5f;

    private NavMeshAgent _navMeshAgent;

    public static PlayerAgent Monster { get; private set; }

    public static readonly List<Transform> Victims = new List<Transform>();
    
    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();

        Debug.Log(Victims.Count);
        if (Monster == null)
            SetAsMonster();
        else
            SetNormal();
       
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, evadeRadius);
        
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, moveRadius);

    
        
        if (_navMeshAgent)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_navMeshAgent.destination, .2f);
            Gizmos.DrawLine(transform.position, _navMeshAgent.destination);
        }
    }

    public void SetMaterial(bool isEnemy) => render.material = isEnemy ? normalMaterial : monsterMaterial;

    public bool IsMonster() => Monster == this;
    void SetNormal()
    {
        if(Monster == this) Monster = null;
        SetMaterial(false);
        Victims.Add(transform);
        name = name.Replace("Monster ", string.Empty);
    }
    
    public void SetAsMonster()
    {
        Victims.Remove(transform);
        SetMaterial(true);
        if (Monster) Monster.SetNormal();
        
        
        Monster = this;
        name = $"Monster {name}";
        _navMeshAgent.destination = transform.position;
        _navMeshAgent.velocity = Vector3.zero;
    }

    public IEnumerable<Transform> Get() => Victims;
    Transform IGetNavAgentTarget.Get() => Monster==null?null:Monster.transform;

    public void Set(bool value)
    {
        if (value)
            SetAsMonster();
        else 
            SetNormal();
    }
}
