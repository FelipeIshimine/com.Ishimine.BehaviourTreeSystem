using System.Collections;
using System.Collections.Generic;
using BehaviourTreeSystem.Runtime.Core;
using TheKiwiCoder;
using UnityEngine;

public class SubTreeNode : Node
{
    public BehaviourTree subTree;

    public override string GetName() => $"{(subTree?subTree.name:"Empty")}";

    protected override void Initialize()
    {
        subTree.Bind(treeRunner);
    }

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State Execution() => subTree.Execute();

    public override Node Clone()
    {
        SubTreeNode nSubTreeNode = (SubTreeNode)base.Clone();
        nSubTreeNode.subTree = subTree.Clone();
        nSubTreeNode.subTree.SetContainerNode(nSubTreeNode);
        foreach (var subTreeNode in nSubTreeNode.subTree.nodes)
            subTreeNode.treeRunner = nSubTreeNode.treeRunner;
        return nSubTreeNode;
    }
}
