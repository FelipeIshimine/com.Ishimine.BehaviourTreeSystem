using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BehaviourTreeSystem.Runtime.Core;
using Newtonsoft.Json;
using TheKiwiCoder;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;
using Blackboard = UnityEditor.Experimental.GraphView.Blackboard;
using Node = BehaviourTreeSystem.Runtime.Core.Node;
using Object = UnityEngine.Object;

namespace BehaviourTreeSystem.Editor {
    public class BehaviourTreeView : GraphView 
    {
        public Action<NodeView> OnNodeSelected;
        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, GraphView.UxmlTraits> { }
        BehaviourTree tree;
        BehaviourTreeSettings settings;

        public BehaviourTree Tree => tree;
        private Blackboard _blackboard;
        
        public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();

        private NodeSearchWindow _searchWindow;
        public struct ScriptTemplate {
            public TextAsset templateFile;
            public string defaultFileName;
            public string subFolder;
        }

        public ScriptTemplate[] scriptFileAssets = {
            
            new ScriptTemplate{ templateFile=BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateActionNode, defaultFileName="NewActionNode.cs", subFolder="Actions" },
            new ScriptTemplate{ templateFile=BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateCompositeNode, defaultFileName="NewCompositeNode.cs", subFolder="Composites" },
            new ScriptTemplate{ templateFile=BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateDecoratorNode, defaultFileName="NewDecoratorNode.cs", subFolder="Decorators" },
        };

        public BehaviourTreeView() 
        {
            settings = BehaviourTreeSettings.GetOrCreateSettings();

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer()); 
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new DoubleClickSelection());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            var styleSheet = settings.behaviourTreeStyle;
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;

            serializeGraphElements = OnSerializedGraphElements;
            unserializeAndPaste = OnUnserializeAndPaste;
        }

        private void OnUnserializeAndPaste(string operationname, string data)
        {
            Debug.Log(operationname);

            string[] nodeData = data.Split('@');

            
            foreach (string nodeIdentificationData in nodeData)
            {
                if(string.IsNullOrEmpty(nodeIdentificationData)) continue;

                Debug.Log(nodeIdentificationData);
                string[] values = nodeIdentificationData.Split('#');
                
                string treeGuid = values[0];
                string nodeGuid = values[1];
                
                BehaviourTree behaviourTree = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(treeGuid)) as BehaviourTree;
                CloneNode(behaviourTree.nodes.Find(x => x.guid == nodeGuid));
            }
        }

        private string OnSerializedGraphElements(IEnumerable<GraphElement> elements)
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            
            foreach (GraphElement graphElement in elements)
            {
                if (graphElement is NodeView nodeView)
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(nodeView.node, out string guid, out long localId);
                    stringBuilder.Append($"@{guid}#{nodeView.node.guid}");
                }
            }
            return stringBuilder.ToString();
        }

        public void AddSearchWindow(EditorWindow window)
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Initialize(this, window);
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        public  void AddBlackboard(EditorWindow window)
      {
          _blackboard = new Blackboard(this);
          _blackboard.Add(new BlackboardSection { title = "Exposed Properties"});
          
          _blackboard.editTextRequested = (_blackboard, element, newValue) =>
          {
              var oldPropertyName = ((BlackboardField) element).text;
              if (ExposedProperties.Any(x => x.PropertyName == newValue))
              {
                  EditorUtility.DisplayDialog("Error", "This property name already exists, please chose another one.",
                      "OK");
                  return;
              }

              var targetIndex = ExposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
              ExposedProperties[targetIndex].PropertyName = newValue;
              ((BlackboardField) element).text = newValue;
          };
          
          _blackboard.SetPosition(new Rect(10,window.position.size.y-220,200,200));
          _blackboard.addItemRequested = x => this.AddPropertyToBlackBoard(ExposedProperty.CreateInstance());
          Add(_blackboard);
      }

        public  void AddMiniMap(EditorWindow window)
      {
          var miniMap = new MiniMap{anchored = false};
          Vector2 v = new Vector2( window.position.size.x-10,30);
          var coords = contentViewContainer.WorldToLocal(v);
          Debug.Log($"coords:{coords}");
          miniMap.SetPosition(new Rect(coords.x,coords.y, 200,140));
          Add(miniMap);
      }
      
    
      public void AddPropertyToBlackBoard(ExposedProperty property, bool loadMode = false)
      {
          var localPropertyName = property.PropertyName;
          var localPropertyValue = property.PropertyValue;
          if (!loadMode)
          {
              while (ExposedProperties.Any(x => x.PropertyName == localPropertyName))
                  localPropertyName = $"{localPropertyName}(1)";
          }

          var item = ExposedProperty.CreateInstance();
          item.PropertyName = localPropertyName;
          item.PropertyValue = localPropertyValue;
          ExposedProperties.Add(item);

          var container = new VisualElement();
          var field = new BlackboardField {text = localPropertyName, typeText = "string"};
          container.Add(field);

          var propertyValueTextField = new TextField("Value:")
          {
              value = localPropertyValue
          };
          propertyValueTextField.RegisterValueChangedCallback(evt =>
          {
              var index = ExposedProperties.FindIndex(x => x.PropertyName == item.PropertyName);
              ExposedProperties[index].PropertyValue = evt.newValue;
          });
          var sa = new BlackboardRow(field, propertyValueTextField);
          container.Add(sa);
          _blackboard.Add(container);
      }
      
        private void OnUndoRedo() {
            PopulateView(tree);
            AssetDatabase.SaveAssets();
        }

        public NodeView FindNodeView(Node node) 
        {
            return GetNodeByGuid(node.guid) as NodeView;
        }

        internal void PopulateView(BehaviourTree tree) {
            this.tree = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            if (tree.rootNode == null) {
                tree.rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
                EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }

            tree.nodes.RemoveAll(x => x == null);
            
            // Creates node view
            tree.nodes.ForEach(CreateNodeView);

            // Create edges
            tree.nodes.ForEach(n => {
                var children = BehaviourTree.GetChildren(n);
                children.ForEach(c => 
                {
                    NodeView parentView = FindNodeView(n);
                    NodeView childView = FindNodeView(c);

                    Edge edge = parentView.output.ConnectTo(childView.input);
                    AddElement(edge);
                });
            });
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            if (graphViewChange.elementsToRemove != null) {
                graphViewChange.elementsToRemove.ForEach(elem => {
                    NodeView nodeView = elem as NodeView;
                    if (nodeView != null) {
                        tree.DeleteNode(nodeView.node);
                    }

                    Edge edge = elem as Edge;
                    if (edge != null) {
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;
                        tree.RemoveChild(parentView.node, childView.node);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null) {
                graphViewChange.edgesToCreate.ForEach(edge => {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    tree.AddChild(parentView.node, childView.node);
                });
            }

            nodes.ForEach((n) => {
                NodeView view = n as NodeView;
                view.SortChildren();
            });

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) 
        {
            base.BuildContextualMenu(evt);
            evt.menu.AppendSeparator();
            // New script functions
            evt.menu.AppendAction($"Create Script.../New Action Node", (a) => CreateNewScript(scriptFileAssets[0]));
            evt.menu.AppendAction($"Create Script.../New Composite Node", (a) => CreateNewScript(scriptFileAssets[1]));
            evt.menu.AppendAction($"Create Script.../New Decorator Node", (a) => CreateNewScript(scriptFileAssets[2]));
        }

        void SelectFolder(string path) {
            // https://forum.unity.com/threads/selecting-a-folder-in-the-project-via-button-in-editor-window.355357/
            // Check the path has no '/' at the end, if it does remove it,
            // Obviously in this example it doesn't but it might
            // if your getting the path some other way.

            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            // Load object
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
        }

        void CreateNewScript(ScriptTemplate template) {
            SelectFolder($"{settings.newNodeBasePath}/{template.subFolder}");
            var templatePath = AssetDatabase.GetAssetPath(template.templateFile);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, template.defaultFileName);
        }

        public void CloneNode(Node originalNode) 
        {
            Node node = tree.CloneNode(originalNode);

            bool ready;
            do
            {
                ready = true;
                node.position += settings.pasteOffset;
                foreach (Node treeNode in tree.nodes)
                    if (treeNode != node && treeNode.position == node.position)
                            ready = false;
            } while (!ready);
            
            CreateNodeView(node);
        }
        
        public void CreateNode(System.Type type, Vector2 position) {
            
            Debug.Log($"Create Node:{type} at position");
            Node node = tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node);
        }

        void CreateNodeView(Node node) {
            NodeView nodeView = new NodeView(node);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }

        public void UpdateNodeStates()
        {
            nodes.ForEach(n => {
                NodeView view = n as NodeView;
                view.UpdateState();
            });
        }

    }
    
    
    
}

[System.Serializable]
public class ExposedProperty
{
    public string PropertyName = "New Name";
    public string PropertyValue = "New Value";
    
    public static ExposedProperty CreateInstance()
    {
        return new ExposedProperty();
    }
}