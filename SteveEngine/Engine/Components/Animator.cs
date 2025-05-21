using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteveEngine
{
    public class Animator : Component
    {
        public AnimationClip CurrentClip { get; private set; }
        public float CurrentTime { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool Loop { get; set; } = true;
        public float Speed { get; set; } = 1.0f;
        public bool IsPaused { get; private set; } = false;
        public bool IsFinished => CurrentTime >= CurrentClip.Length;
        public Animator()
        {
            CurrentClip = null;
            CurrentTime = 0.0f;
            IsPlaying = false;
        }
        public void Play(AnimationClip clip)
        {
            if (CurrentClip != clip)
            {
                CurrentClip = clip;
                CurrentTime = 0.0f;
                IsPlaying = true;
            }
            else
            {
                IsPlaying = true;
            }
        }
        public void Pause()
        {
            IsPaused = true;
        }
        public void Resume()
        {
            IsPaused = false;
        }
        public void Stop()
        {
            IsPlaying = false;
            CurrentTime = 0.0f;
        }
        public void Update(float deltaTime)
        {
            if (IsPlaying && !IsPaused)
            {
                CurrentTime += deltaTime * Speed;
                if (CurrentTime >= CurrentClip.Length)
                {
                    if (Loop)
                    {
                        CurrentTime = 0.0f;
                    }
                    else
                    {
                        IsPlaying = false;
                        CurrentTime = CurrentClip.Length;
                    }
                }
            }
            base.Update(deltaTime);
        }
        public Matrix4 GetCurrentTransform()
        {
            if (CurrentClip == null || CurrentClip.Keyframes.Count == 0)
                return Matrix4.Identity;

            AnimationKeyframe previousKeyframe = null;
            AnimationKeyframe nextKeyframe = null;
            foreach (var keyframe in CurrentClip.Keyframes)
            {
                if (keyframe.Time <= CurrentTime)
                {
                    previousKeyframe = keyframe;
                }
                else
                {
                    nextKeyframe = keyframe;
                    break;
                }
            }
            if (previousKeyframe == null)
                return nextKeyframe.Transform;
            if (nextKeyframe == null)
                return previousKeyframe.Transform;
            // Interpolate between the two keyframes
            float t = (CurrentTime - previousKeyframe.Time) / (nextKeyframe.Time - previousKeyframe.Time);
            return Lerp(previousKeyframe.Transform, nextKeyframe.Transform, t);
        }
        public static Matrix4 Lerp(Matrix4 start, Matrix4 end, float t)
        {
            t = Math.Clamp(t, 0.0f, 1.0f);
            return new Matrix4(
                Vector4.Lerp(start.Row0, end.Row0, t),
                Vector4.Lerp(start.Row1, end.Row1, t),
                Vector4.Lerp(start.Row2, end.Row2, t),
                Vector4.Lerp(start.Row3, end.Row3, t)
            );
        }
    }
}
