using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class Breakpoint : ActionNode
{
    public string debugText = "Trigging Breakpoint";
    
    public override string NodeDescription => $"Executes a Debug.Log({debugText}) then executes a Debug.Break(). Always returns SUCCESS";

    protected override void Initialize() { }

    protected override void OnStart() 
    {
        Debug.Log(debugText);
        Debug.Break();
    }

    protected override void OnStop() { }

    protected override State Execution() => State.Success;
}
