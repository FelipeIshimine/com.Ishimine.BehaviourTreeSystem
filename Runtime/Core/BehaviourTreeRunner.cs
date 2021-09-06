using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTreeSystem.Runtime.Core {
    public class BehaviourTreeRunner : MonoBehaviour
    {
        public bool playOnStart = false;
        public static event Action<Node> OnNodeExecution;

        private string lastNodeExecuted;

        public BehaviourTree defaultTree;

        // The main behaviour tree asset
        private BehaviourTree _currentTree;
        public BehaviourTree CurrentTree => _currentTree;

        private readonly Dictionary<Type, object> _componentCache = new Dictionary<Type, object>();

        private Node.State _lastState = Node.State.Running;
        
        // Start is called before the first frame update
        void Start() 
        {
            if(playOnStart && defaultTree) Set(defaultTree);
        }
        
        public void Set(BehaviourTree newTree)
        {
            _lastState = Node.State.Running;
            if(_currentTree) Destroy(_currentTree);
            
            _currentTree = newTree.Clone();
            _currentTree.Bind(this);
        }

        // Update is called once per frame
        void Update() 
        {
            if(_lastState != Node.State.Running) return;
            
            if (_currentTree) 
                _lastState = _currentTree.Execute();
        }

        private void OnDrawGizmosSelected() {
            if (!_currentTree) {
                return;
            }

            BehaviourTree.Traverse(_currentTree.rootNode, (n) => {
                if (n.drawGizmos) {
                    n.OnDrawGizmos();
                }
            });
        }

        public T GetCacheComponent<T>() where T : class
        {
            if(_componentCache.TryGetValue(typeof(T), out object component)) return component as T;
            component = GetComponent<T>();
            _componentCache.Add(typeof(T),component);
            return component as T;
        }
        
        public T GetCacheComponentInChildren<T>(bool includeInactive) where T : class
        {
            if(_componentCache.TryGetValue(typeof(T), out object component)) return component as T;
            component = GetComponentInChildren<T>(includeInactive);
            _componentCache.Add(typeof(T),component);
            return component as T;
        }
        
        public T GetCacheComponentInParent<T>() where T : class
        {
            if(_componentCache.TryGetValue(typeof(T), out object component)) return component as T;
            component = GetComponentInParent<T>();
            _componentCache.Add(typeof(T),component);
            return component as T;
        }
        

        public void NodeExecuted(Node node)
        {
            lastNodeExecuted = node.GetName();
            OnNodeExecution?.Invoke(node);
        }
        
        private void OnDrawGizmos()
        {
            if (!CurrentTree) return;
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up, $"<color=white>{CurrentTree.name} \n {lastNodeExecuted}</color>", new GUIStyle(){richText = true});
#endif
        }

        
    }
}