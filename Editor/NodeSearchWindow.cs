using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourTreeSystem.Editor;
using BehaviourTreeSystem.Runtime.Core;
using TheKiwiCoder;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private BehaviourTreeView _graphView;
    private EditorWindow _window;

    public void Initialize(BehaviourTreeView graphView, EditorWindow window)
    {
        _graphView = graphView;
        _window = window;

    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0)
        };


        #region Actions

        var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
        tree.Add(new SearchTreeGroupEntry(new GUIContent(" Action"), 1));
        foreach (var type in types)
        {
            if (type.IsAbstract) continue;
            tree.Add(new SearchTreeEntry(new GUIContent(type.Name))
            {
                userData = type, level = 2
            });
        }

        #endregion

        #region Composite

        types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
        tree.Add(new SearchTreeGroupEntry(new GUIContent(" Composite"), 1));
        foreach (var type in types)
        {
            if (type.IsAbstract) continue;
            tree.Add(new SearchTreeEntry(new GUIContent(type.Name))
            {
                userData = type, level = 2
            });
        }

        #endregion

        #region Decorators

        types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
        tree.Add(new SearchTreeGroupEntry(new GUIContent(" Decorator"), 1));
        foreach (var type in types)
        {
            if (type.IsAbstract) continue;
            tree.Add(new SearchTreeEntry(new GUIContent(type.Name))
            {
                userData = type, level = 2
            });
        }

        #endregion

        #region Conditional

        types = TypeCache.GetTypesDerivedFrom<ConditionalNode>();
        tree.Add(new SearchTreeGroupEntry(new GUIContent(" Conditional"), 1));
        foreach (var type in types)
        {
            if (type.IsAbstract) continue;
            tree.Add(new SearchTreeEntry(new GUIContent(type.Name))
            {
                userData = type, level = 2
            });
        }

        #endregion
        
        #region SubTree

        tree.Add(new SearchTreeEntry(new GUIContent($" {nameof(SubTreeNode)}"))
        {
            userData = typeof(SubTreeNode), level = 1
        });

        #endregion

        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var worldPosCoordinate = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
            context.screenMousePosition - _window.position.position);

        var localPosition = _graphView.contentViewContainer.WorldToLocal(worldPosCoordinate);
        
        _graphView.CreateNode(SearchTreeEntry.userData as Type, localPosition);
        return true;
    }
}
