using Monocle;
using Microsoft.Xna.Framework;
using System;

namespace CelestialLeague.Client.Motion
{
    public class Spring
    {
        public float Value { get; private set; }
        public float Velocity { get; private set; }
        
        public float Target { get; set; }
        
        public float Stiffness = 100f;  // how fast it will travel
        public float Damping = 10f; // bounciness
        
        public Spring(float initialValue = 0f)
        {
            Value = initialValue;
            Target = initialValue;
            Velocity = 0f;
        }
        
        public void Update(float deltaTime)
        {
            // physics: force pulls toward target, damping slows down
            float force = (Target - Value) * Stiffness - Velocity * Damping;
            
            Velocity += force * deltaTime;
            Value += Velocity * deltaTime;
        }
        
        public bool IsSettled(float threshold = 0.01f)
        {
            return Math.Abs(Value - Target) < threshold && Math.Abs(Velocity) < threshold;
        }
        
        public void SetValue(float value)
        {
            Value = value;
        }
        
        public void Reset(float value)
        {
            Value = value;
            Target = value;
            Velocity = 0f;
        }
    }
}