using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGetNavAgentTargets
{
    public IEnumerable<Transform> Get();
}