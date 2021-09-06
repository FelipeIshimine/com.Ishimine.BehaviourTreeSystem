using System;
using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;

public class ParallelSequencer : CompositeNode
{
    private int _current;

    public override string NodeDescription => "Executes children one at a time left to right. Advance when child returns SUCCESS. Every child that has SUCCEEDED keeps being evaluated every tick, if it FAILS, the execution goes back and any subsequent child is aborted. \nSUCCESS:When every children returns SUCCESS\n:FAILURE:When the first child returns FAILURE";

    protected override void Initialize() { }

    protected override void OnStart()
    {
        _current = 0;
    }

    protected override void OnStop()
    {
    }

    protected override State Execution()
    {
        bool fail = false;
        for (int i = 0; i < children.Count; i++)
        {
            if(fail)
            {
                if(children[i].state == State.Running) children[i].Abort();
                continue;
            }
            
            var value = children[i].Execute();
            switch (value)
            {
                case State.Running:
                {
                    if(i < _current) return value;
                    break;
                }
                case State.Failure:
                {
                    _current = i-1;
                    fail = true;
                    children[i].Abort();
                    break;
                }
                case State.Success:
                {
                    if (i > _current) _current = i;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (_current < 0) return State.Failure;
        return _current < children.Count ? State.Running : State.Success;
    }
}
