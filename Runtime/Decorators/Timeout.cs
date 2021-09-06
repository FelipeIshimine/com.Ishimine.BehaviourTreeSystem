using System;
using BehaviourTreeSystem.Runtime.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BehaviourTreeSystem.Runtime 
{
    public class Timeout : ConditionalNode 
    {
        public float duration = 1.0f;
        
        public Vector2 rangeVariation = new Vector2();

        private float _startTime;

        private float _currentDuration;

        public bool printDebug = false;

        public override string NodeDescription => "Aborts it's child if it hasn't finished after a given timeout period.";
        public override string GetName()=> $"{base.GetName()} \n {GetNodeTextDuration()}";

        private string GetNodeTextDuration() => !Application.isPlaying ? ($"{duration:F} [-{rangeVariation.x},+{rangeVariation.y}] ") : 
            Mathf.Max((_currentDuration - (Time.time - _startTime)),0).ToString("F");

        protected override void Initialize() { }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (rangeVariation.x < 0)
                rangeVariation.x = 0;
            
            if (rangeVariation.x > duration)
                rangeVariation.x = duration;

        }

        protected override void OnStart() 
        {
            _startTime = Time.time;
            _currentDuration = Mathf.Max(0,duration + Random.Range(rangeVariation.x, rangeVariation.y));
        }

        protected override void OnStop() {
        }

        protected override State Condition()=> IsTimeout() ? State.Failure : State.Success;
      
        private bool IsTimeout()=> Time.time - _startTime > _currentDuration;

      
    }
}