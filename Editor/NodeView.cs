using System;
using BehaviourTreeSystem.Runtime.Core;
using TheKiwiCoder;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Node = BehaviourTreeSystem.Runtime.Core.Node;

namespace BehaviourTreeSystem.Editor 
{

    public class NodeView : UnityEditor.Experimental.GraphView.Node {
        public Action<NodeView> OnNodeSelected;
        public Node node;
        public Port input;
        public Port output;

        public NodeView(Node node) : base(AssetDatabase.GetAssetPath(BehaviourTreeSettings.GetOrCreateSettings().nodeXml)) {
            this.node = node;

            UpdateName();

            this.viewDataKey = node.guid;
            style.left = node.position.x;
            style.top = node.position.y;
            

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            SetupDataBinding();
        }

        private void UpdateName() => this.title = this.node.name =
            ((string.IsNullOrEmpty(node.OverrideName)) ? node.GetName() : node.OverrideName).Replace("Node",string.Empty);

        private void SetupDataBinding() {
            Label descriptionLabel = this.Q<Label>("description");
            descriptionLabel.bindingPath = "description";
            descriptionLabel.Bind(new SerializedObject(node));
        }

        private void SetupClasses()
        {
            switch (node)
            {
                case SubTreeNode _:
                    AddToClassList("subtree");
                    break;
                case ActionNode _:
                    AddToClassList("action");
                    break;
                case CompositeNode _:
                    AddToClassList("composite");
                    break;
                case DecoratorNode _:
                    AddToClassList("decorator");
                    break;
                case ConditionalNode _:
                    AddToClassList("conditional");
                    break;
                case RootNode _:
                    AddToClassList("root");
                    break;
            }
        }
        

        private void CreateInputPorts()
        {
            switch (node)
            {
                case ActionNode _:
                case CompositeNode _:
                case DecoratorNode _:
                case SubTreeNode _:
                case ConditionalNode _:
                    input = new NodePort(Direction.Input, Port.Capacity.Single);
                    break;
                case RootNode _:
                    break;
            }

            if (input != null) {
                input.portName = "";
                input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }

        private void CreateOutputPorts()
        {
            switch (node)
            {
                case ActionNode _:
                case SubTreeNode _:
                    break;
                case CompositeNode _:
                    output = new NodePort(Direction.Output, Port.Capacity.Multi);
                    break;
                case ConditionalNode _:
                case DecoratorNode _:
                case RootNode _:
                    output = new NodePort(Direction.Output, Port.Capacity.Single);
                    break;
            }

            if (output != null) {
                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            Undo.RecordObject(node, "Behaviour Tree (Set Position");
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;
            EditorUtility.SetDirty(node);
        }

        public override void OnSelected() {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }

        public void SortChildren()
        {
            if (node is CompositeNode composite) {
                composite.children.RemoveAll(x => x == null);
                composite.children.Sort(SortByHorizontalPosition);
            }
        }

        private int SortByHorizontalPosition(Node left, Node right) {
            return left.position.x < right.position.x ? -1 : 1;
        }

        public void UpdateState() 
        {
            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");
            RemoveFromClassList("disable");

            UpdateName();


            if (Application.isPlaying)
            {
                if (!node.executed)
                {
                    RemoveFromClassList("action");
                    RemoveFromClassList("composite");
                    RemoveFromClassList("decorator");
                    RemoveFromClassList("root");
                    RemoveFromClassList("conditional");
                    
                    AddToClassList("disable");
                    return;
                }
                
                SetupClasses();
                switch (node.state)
                {
                    case Node.State.Running:
                        AddToClassList("running");
                        break;
                    case Node.State.Failure:
                        AddToClassList("failure");
                        break;
                    case Node.State.Success:
                        AddToClassList("success");
                        break;
                    case Node.State.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            


        }
    }
}