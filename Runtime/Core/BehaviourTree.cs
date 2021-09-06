using System.Collections.Generic;
using TheKiwiCoder;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
#endif


namespace BehaviourTreeSystem.Runtime.Core {
    [CreateAssetMenu()]
    public class BehaviourTree : ScriptableObject 
    {
        public Node rootNode;
        public List<Node> nodes = new List<Node>();
        public Blackboard blackboard = new Blackboard();

        public SubTreeNode containerNode;
        public bool IsSubTree => containerNode != null;

        public void SetContainerNode(SubTreeNode subTreeNode)
        {
            containerNode = subTreeNode;
        }
        
        public Node.State Execute() 
        {
            foreach (Node node in nodes) node.executed = false;
            return rootNode.Execute();
        }

        public static List<Node> GetChildren(Node parent) 
        {
            List<Node> children = new List<Node>();

            switch (parent)
            {
                case DecoratorNode decorator when decorator.child != null:
                    children.Add(decorator.child);
                    break;
                case ConditionalNode conditional when conditional.Child != null:
                    children.Add(conditional.Child);
                    break;
                case RootNode rootNode when rootNode.child != null:
                    children.Add(rootNode.child);
                    break;
                case CompositeNode composite:
                    return composite.children;
            }

            return children;
        }

        public static void Traverse(Node node, System.Action<Node> visiter) 
        {
            if (node) 
            {
                visiter.Invoke(node);
                var children = GetChildren(node);
                children.ForEach((n) => Traverse(n, visiter));
            }
        }

        public BehaviourTree Clone() {
            BehaviourTree tree = Instantiate(this);
            tree.rootNode = tree.rootNode.Clone();
            tree.nodes = new List<Node>();
            Traverse(tree.rootNode, (n) => {
                tree.nodes.Add(n);
            });

            return tree;
        }

        public void Bind(BehaviourTreeRunner treeRunner) {
            Traverse(rootNode, node => 
            {
                node.Initialize(treeRunner, this);
                node.blackboard = blackboard;
            });
        }


        #region Editor Compatibility
#if UNITY_EDITOR

        public Node CreateNode(System.Type type) 
        {
            Debug.Log($"Creating:{type.Name}");
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
            nodes.Add(node);

            if (!Application.isPlaying) {
                AssetDatabase.AddObjectToAsset(node, this);
            }

            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");

            AssetDatabase.SaveAssets();
            return node;
        }

        public Node CloneNode(Node node)
        {
            Debug.Log($"Cloning:{node}");
            Node nNode = Instantiate(node);
            nNode.name = node.name;
            nNode.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
            nodes.Add(nNode);

            if (!Application.isPlaying) {
                AssetDatabase.AddObjectToAsset(nNode, this);
            }

            Undo.RegisterCreatedObjectUndo(nNode, "Behaviour Tree (CreateNode)");
            AssetDatabase.SaveAssets();
            return nNode;
        }
        
        
        public void DeleteNode(Node node) {
            Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            nodes.Remove(node);

            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);

            AssetDatabase.SaveAssets();
        }

        public void AddChild(Node parent, Node child) 
        {
            switch (parent)
            {
                case DecoratorNode decorator:
                    Undo.RecordObject(decorator, "Behaviour Tree (AddChild)");
                    decorator.child = child;
                    EditorUtility.SetDirty(decorator);
                    break;
                case ConditionalNode conditional:
                    Undo.RecordObject(conditional, "Behaviour Tree (AddChild)");
                    conditional.Child = child;
                    EditorUtility.SetDirty(conditional);
                    break;
                case RootNode root:
                    Undo.RecordObject(root, "Behaviour Tree (AddChild)");
                    root.child = child;
                    EditorUtility.SetDirty(root);
                    break;
                case CompositeNode composite:
                    Undo.RecordObject(composite, "Behaviour Tree (AddChild)");
                    composite.children.Add(child);
                    EditorUtility.SetDirty(composite);
                    break;
            }
        }

        public void RemoveChild(Node parent, Node child) {
            switch (parent)
            {
                case DecoratorNode decorator:
                    Undo.RecordObject(decorator, "Behaviour Tree (RemoveChild)");
                    decorator.child = null;
                    EditorUtility.SetDirty(decorator);
                    break;
                case ConditionalNode conditional:
                    Undo.RecordObject(conditional, "Behaviour Tree (RemoveChild)");
                    conditional.Child = null;
                    EditorUtility.SetDirty(conditional);
                    break;
                case RootNode rootNode:
                    Undo.RecordObject(rootNode, "Behaviour Tree (RemoveChild)");
                    rootNode.child = null;
                    EditorUtility.SetDirty(rootNode);
                    break;
                case CompositeNode composite:
                    Undo.RecordObject(composite, "Behaviour Tree (RemoveChild)");
                    composite.children.Remove(child);
                    EditorUtility.SetDirty(composite);
                    break;
            }
        }
#endif
        #endregion Editor Compatibility

        public BehaviourTree GetParentTree() => containerNode.tree;
    }
}