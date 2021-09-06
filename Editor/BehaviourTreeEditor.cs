using System;
using System.Collections.Generic;
using BehaviourTreeSystem.Runtime.Core;
using BehaviourTreeSystem.Editor;
using TheKiwiCoder;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTreeSystem.Editor
{

    public class BehaviourTreeEditor : EditorWindow
    {
        private BehaviourTreeView _graphView;
        private BehaviourTree _tree;
        private InspectorView _inspectorView;
        private IMGUIContainer _blackboardView;
        private ToolbarMenu _toolbarMenu;
        private TextField _treeNameField;
        private TextField _locationPathField;
        private Label _descriptionLabel;
        private Foldout _foldout;
        private Button _createNewTreeButton;
        private VisualElement _overlay;
        private BehaviourTreeSettings _settings;
        //private MiniMap _miniMap;

        private SerializedObject _treeObject;
        private SerializedProperty _blackboardProperty;

        public const string WindowName = "BehaviourTreeEditor";

        [MenuItem("Window/BehaviorTreeSystem")]
        public static void OpenWindow()
        {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent(WindowName);

        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is BehaviourTree)
            {
                OpenWindow();
                return true;
            }

            return false;
        }

        List<T> LoadAssets<T>() where T : UnityEngine.Object
        {
            string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            List<T> assets = new List<T>();
            foreach (var assetId in assetIds)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetId);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }

            return assets;
        }
        
        

        public void CreateGUI()
        {

            _settings = BehaviourTreeSettings.GetOrCreateSettings();

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = _settings.behaviourTreeXml;
            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = _settings.behaviourTreeStyle;
            root.styleSheets.Add(styleSheet);

            // Main treeview
            _graphView = root.Q<BehaviourTreeView>();
            _graphView.OnNodeSelected = OnNodeSelectionChanged;

            
            // Inspector View
            _inspectorView = root.Q<InspectorView>();

            _descriptionLabel = root.Q<Label>("NodeDescription");
            _descriptionLabel.text = "None";

            _foldout = root.Q<Foldout>();
            _foldout.text = "Functionality:";
            _foldout.visible = !string.IsNullOrEmpty(_foldout.text); 
            _foldout.SetValueWithoutNotify(false);

            // Blackboard view
            _blackboardView = root.Q<IMGUIContainer>();
            if (_blackboardView != null)
            {
                _blackboardView.onGUIHandler = () =>
                {
                    if (_treeObject != null && _treeObject.targetObject != null)
                    {
                        _treeObject.Update();
                        EditorGUILayout.PropertyField(_blackboardProperty);
                        _treeObject.ApplyModifiedProperties();
                    }
                };
            }
            // Toolbar assets menu
            _toolbarMenu = root.Q<ToolbarMenu>();
            var behaviourTrees = LoadAssets<BehaviourTree>();
            behaviourTrees.ForEach(tree =>
            {
                _toolbarMenu.menu.AppendAction($"{tree.name}", (a) => { Selection.activeObject = tree; });
            });
            _toolbarMenu.menu.AppendSeparator();
            _toolbarMenu.menu.AppendAction("New Tree...", (a) => CreateNewTree("NewBehaviourTree"));

            // New Tree Dialog
            _treeNameField = root.Q<TextField>("TreeName");
            _locationPathField = root.Q<TextField>("LocationPath");
            _overlay = root.Q<VisualElement>("Overlay");
            _createNewTreeButton = root.Q<Button>("CreateButton");
            _createNewTreeButton.clicked += () => CreateNewTree(_treeNameField.value);

            if (_tree == null)
            {
                OnSelectionChange();
            }
            else
            {
                SelectTree(_tree);
            }
            
            _graphView.AddSearchWindow(this);
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            Debug.Log($"OnPlayModeStateChanged:{obj}");
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    var trees = LoadAssets<BehaviourTree>();
                    _tree = trees.Count > 0 ? trees[0] : null;
                    SelectTree(_tree);
                    break;
            }
        }

        private void OnSelectionChange()
        {
            void DelayCall()
            {
                BehaviourTree auxTree = Selection.activeObject as BehaviourTree;
                if (!auxTree)
                {
                    if (Selection.activeGameObject)
                    {
                        BehaviourTreeRunner runner = Selection.activeGameObject.GetComponent<BehaviourTreeRunner>();
                        if (runner)
                            auxTree = runner.CurrentTree;
                    }
                }

                SelectTree(auxTree);
            }

            EditorApplication.delayCall -= DelayCall;
            EditorApplication.delayCall += DelayCall;
        }

        public void SelectTree(BehaviourTree newTree)
        {

            if (_graphView == null)
            {
                return;
            }

            if (!newTree)
            {
                this.titleContent.text = WindowName;
                return;
            }

            this._tree = newTree;

            this.titleContent.text = $"{_tree.name}";
            _overlay.style.visibility = Visibility.Hidden;

            if (Application.isPlaying)
            {
                _graphView.PopulateView(_tree);
            }
            else
            {
                _graphView.PopulateView(_tree);
            }


            _treeObject = new SerializedObject(_tree);
            _blackboardProperty = _treeObject.FindProperty("blackboard");

            EditorApplication.delayCall += () => { _graphView.FrameAll(); };
        }

        void OnNodeSelectionChanged(NodeView node)
        {
            _inspectorView.UpdateSelection(node);
            _descriptionLabel.text = node!=null?node.node.NodeDescription:string.Empty;
        }

        private void OnInspectorUpdate()
        {
            _graphView?.UpdateNodeStates();
        }

        void CreateNewTree(string assetName)
        {
            string path = System.IO.Path.Combine(_locationPathField.value, $"{assetName}.asset");
            BehaviourTree tree = CreateInstance<BehaviourTree>();
            tree.name = _treeNameField.ToString();
            AssetDatabase.CreateAsset(tree, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = tree;
            EditorGUIUtility.PingObject(tree);
        }
    }
}