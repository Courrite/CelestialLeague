using Microsoft.Xna.Framework;
using System;

namespace CelestialLeague.Client.Motion
{
    public class Elastic
    {
        public Vector2 CurrentValue { get; private set; }
        public Vector2 TargetValue { get; set; }
        
        public Vector2 MinBounds { get; set; } = Vector2.Zero;
        public Vector2 MaxBounds { get; set; } = Vector2.Zero;
        
        public float ElasticAmplitude { get; set; } = 1f; // how much overshoot
        public float ElasticPeriod { get; set; } = 0.3f; // oscillation period
        public float InterpolationSpeed { get; set; } = 8f; // how fast to reach target
        public float BoundaryReturnStrength { get; set; } = 0.15f; // force back to bounds
        
        public bool EnforceBounds { get; set; } = true;
        
        private float interpolationProgress = 0f;
        private Vector2 startValue;
        private bool needsNewInterpolation = true;
        
        public Elastic(Vector2 initialValue = default)
        {
            CurrentValue = initialValue;
            TargetValue = initialValue;
            startValue = initialValue;
        }
        
        public void Update(float deltaTime)
        {
            if (EnforceBounds)
            {
                ApplyBoundaryCorrection();
            }
            
            if (needsNewInterpolation || Vector2.Distance(CurrentValue, TargetValue) > 0.01f)
            {
                if (needsNewInterpolation)
                {
                    startValue = CurrentValue;
                    interpolationProgress = 0f;
                    needsNewInterpolation = false;
                }
                
                interpolationProgress += InterpolationSpeed * deltaTime;
                interpolationProgress = Math.Min(interpolationProgress, 1f);
                
                float easedProgress = ElasticEaseOut(interpolationProgress);
                CurrentValue = Vector2.Lerp(startValue, TargetValue, easedProgress);
                
                if (Vector2.Distance(TargetValue, startValue + (TargetValue - startValue)) > 0.1f)
                {
                    needsNewInterpolation = true;
                }
            }
        }
        
        private void ApplyBoundaryCorrection()
        {
            Vector2 correctedTarget = TargetValue;
            
            if (TargetValue.X < MinBounds.X)
            {
                correctedTarget.X = MathHelper.Lerp(TargetValue.X, MinBounds.X, BoundaryReturnStrength);
            }
            else if (TargetValue.X > MaxBounds.X)
            {
                correctedTarget.X = MathHelper.Lerp(TargetValue.X, MaxBounds.X, BoundaryReturnStrength);
            }
            
            if (TargetValue.Y < MinBounds.Y)
            {
                correctedTarget.Y = MathHelper.Lerp(TargetValue.Y, MinBounds.Y, BoundaryReturnStrength);
            }
            else if (TargetValue.Y > MaxBounds.Y)
            {
                correctedTarget.Y = MathHelper.Lerp(TargetValue.Y, MaxBounds.Y, BoundaryReturnStrength);
            }
            
            TargetValue = correctedTarget;
        }
        
        private float ElasticEaseOut(float t)
        {
            if (t == 0f || t == 1f) return t;
            
            float p = ElasticPeriod;
            float a = ElasticAmplitude;
            
            if (a < 1f)
            {
                a = 1f;
                p = ElasticPeriod / 4f;
            }
            else
            {
                p = ElasticPeriod / (2f * MathF.PI) * MathF.Asin(1f / a);
            }
            
            return a * MathF.Pow(2f, -10f * t) * MathF.Sin((t - p) * (2f * MathF.PI) / ElasticPeriod) + 1f;
        }
        
        public bool IsSettled(float threshold = 0.5f)
        {
            return Vector2.Distance(CurrentValue, TargetValue) < threshold && interpolationProgress >= 0.95f;
        }
        
        public void SetCurrentValue(Vector2 value)
        {
            CurrentValue = value;
            needsNewInterpolation = true;
        }
        
        public void SetTarget(Vector2 target)
        {
            if (Vector2.Distance(TargetValue, target) > 0.01f)
            {
                TargetValue = target;
                needsNewInterpolation = true;
            }
        }
        
        public void AddToTarget(Vector2 delta)
        {
            SetTarget(TargetValue + delta);
        }
        
        public void Reset(Vector2 value)
        {
            CurrentValue = value;
            TargetValue = value;
            startValue = value;
            interpolationProgress = 1f;
            needsNewInterpolation = false;
        }
        
        public void SetBounds(Vector2 min, Vector2 max)
        {
            MinBounds = min;
            MaxBounds = max;
        }
        
        public void SetBounds(float minX, float minY, float maxX, float maxY)
        {
            MinBounds = new Vector2(minX, minY);
            MaxBounds = new Vector2(maxX, maxY);
        }
        
        public void SnapToTarget()
        {
            CurrentValue = TargetValue;
            interpolationProgress = 1f;
            needsNewInterpolation = false;
        }
    }
}