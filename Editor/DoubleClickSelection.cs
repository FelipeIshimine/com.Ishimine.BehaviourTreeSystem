using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourTreeSystem.Runtime.Core;
using BehaviourTreeSystem.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace TheKiwiCoder {
    public class DoubleClickSelection : MouseManipulator 
    {
        double time;
        double doubleClickDuration = 0.3;

        public DoubleClickSelection() {
            time = EditorApplication.timeSinceStartup;
        }

        protected override void RegisterCallbacksOnTarget() {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget() {

            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt) 
        {
            var graphView = target as BehaviourTreeView;
            if (graphView == null)
                return;

            double duration = EditorApplication.timeSinceStartup - time;
            if (duration < doubleClickDuration) {
                SelectChildren(evt);
            }

            time = EditorApplication.timeSinceStartup;
        }

        void SelectChildren(MouseDownEvent evt) 
        {
            var graphView = target as BehaviourTreeView;
            if (graphView == null)
                return;

            if (!CanStopManipulation(evt))
                return;

            NodeView nodeView = evt.target as NodeView;
            if (nodeView == null) 
            {
                var ve = evt.target as VisualElement;
                nodeView = ve.GetFirstAncestorOfType<NodeView>();
                if (nodeView == null)
                {
                    Debug.Log(graphView.Tree);
                    Debug.Log(graphView.Tree.IsSubTree);
                    if (graphView.Tree.IsSubTree)
                    {
                        BehaviourTreeEditor behaviourTreeEditor = EditorWindow.GetWindow<BehaviourTreeEditor>();
                        behaviourTreeEditor.SelectTree(graphView.Tree.GetParentTree());
                    }
                    Debug.Log("Nothing");
                    return;
                }
            }

            if (nodeView.node is SubTreeNode subTreeNode)
            {
                BehaviourTreeEditor behaviourTreeEditor = EditorWindow.GetWindow<BehaviourTreeEditor>();
                behaviourTreeEditor.SelectTree(subTreeNode.subTree);
            }
            else
            {
                // Add children to selection so the root element can be moved
                BehaviourTree.Traverse(nodeView.node, node =>
                {
                    var view = graphView.FindNodeView(node);
                    graphView.AddToSelection(view);
                });
            }
        }
    }
}