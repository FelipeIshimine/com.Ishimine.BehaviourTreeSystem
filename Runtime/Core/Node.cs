using System;
using TheKiwiCoder;
using UnityEngine;

namespace BehaviourTreeSystem.Runtime.Core {
    public abstract class Node : ScriptableObject {
        public enum State {
            None = 0,
            Failure = -1,
            Success = 1,
            Running = 2
        }

        public State state = State.Running;
        
        [SerializeField,TextArea(minLines:1,maxLines:2)] protected string overrideName = string.Empty;
        public virtual string OverrideName => overrideName;
        [HideInInspector] public bool executed = false;
        [HideInInspector] public bool started = false;
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
        [HideInInspector] public BehaviourTreeRunner treeRunner;
        [HideInInspector] public BehaviourTree tree;
        [HideInInspector] public Blackboard blackboard;
        [TextArea] public string description;

        public virtual string NodeDescription { get; } = string.Empty;
        
        public bool drawGizmos = false;

        protected virtual string Description => description;

        protected bool Initialized => treeRunner != null;

        public void Initialize(BehaviourTreeRunner nTreeRunner, BehaviourTree nTree)
        {
            tree = nTree;
            this.treeRunner = nTreeRunner;
            Initialize();
        }

        protected abstract void Initialize();

        protected T GetComponent<T>() where T : class => treeRunner.GetCacheComponent<T>();
        protected T GetComponent<T>(out T aux) where T : class => aux = treeRunner.GetCacheComponent<T>();
        
        protected T GetComponentInChildren<T>(bool includeInactive) where T : class => treeRunner.GetCacheComponentInChildren<T>(includeInactive);
        protected T GetComponentInChildren<T>(out T aux, bool includeInactive) where T : class => aux = treeRunner.GetCacheComponentInChildren<T>(includeInactive);
        
        protected T GetComponentInParent<T>() where T : class => treeRunner.GetCacheComponentInParent<T>();
        protected T GetComponentInParent<T>(out T aux) where T : class => aux = treeRunner.GetCacheComponentInParent<T>();
        
        
        public State Execute()
        {
            if (executed)
                throw new Exception("Node already executed");
            executed = true;
            if (!started) {
                OnStart();
                started = true;
            }

            treeRunner.NodeExecuted(this);
            state = Execution();

            if (state != State.Running) {
                OnStop();
                started = false;
            }

            return state;
        }

        public virtual Node Clone() {
            return Instantiate(this);
        }

        public void Abort() {
            BehaviourTree.Traverse(this, (node) => {
                node.started = false;
                node.state = State.None;
                node.OnStop();
            });
        }

        public virtual void OnDrawGizmos() { }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State Execution();

        public virtual string GetName() => GetType().Name;


        protected virtual void OnValidate() { }
    }
}