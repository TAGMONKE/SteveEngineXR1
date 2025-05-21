using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteveEngine
{
    public class AnimationKeyframe
    {
        public float Time { get; set; }
        public Matrix4 Transform { get; set; }

        public AnimationKeyframe(float time, Matrix4 transform)
        {
            Time = time;
            Transform = transform;
        }
    }

    public class AnimationClip
    {
        public float Length { get; set; }
        public List<AnimationKeyframe> Keyframes { get; set; }

        public AnimationClip(float length)
        {
            Length = length;
            Keyframes = new List<AnimationKeyframe>();
        }

        public void AddKeyframe(float time, Matrix4 transform)
        {
            Keyframes.Add(new AnimationKeyframe(time, transform));
        }

        public void RemoveKeyframe(float time)
        {
            Keyframes.RemoveAll(k => k.Time == time);
        }
    }
}
