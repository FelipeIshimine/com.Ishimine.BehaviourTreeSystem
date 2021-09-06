using System;
using BehaviourTreeSystem.Runtime.Core;
using TheKiwiCoder;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BehaviourTreeSystem.Runtime {
    public class Wait : ConditionalNode 
    {
        public float duration = 1;

        public Vector2 rangeVariation = new Vector2();

        private float _startTime;

        private float _currentDuration;
        
        public override string GetName()=> $"{base.GetName()} \n {GetNodeTextDuration()}";

        private string GetNodeTextDuration() => !Application.isPlaying || !executed ? ($"{duration:F} [-{rangeVariation.x},+{rangeVariation.y}] ") : Mathf.Abs(Time.time - _startTime - _currentDuration).ToString("F");

        public override string NodeDescription => $"Waits for {duration} seconds and then executes the child node \nRunning: During countdown\nSuccessOrFailure: Depends on child node after the countdown is finished";

        protected override void Initialize()
        {
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (rangeVariation.x < 0)
                rangeVariation.x = 0;

            if (rangeVariation.x > duration)
                rangeVariation.x = duration;

        }

        protected override void OnStart() {
            _startTime = Time.time;
            _currentDuration = Mathf.Max(0,duration + Random.Range(rangeVariation.x, rangeVariation.y));
        }

        protected override void OnStop() { }

        protected override State Condition() => IsWaitDone() ? State.Success : State.Running;

        private bool IsWaitDone()=> Time.time - _startTime > _currentDuration;

    }
}
