using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;


namespace SteveEngine
{
    public class AudioListener : Component
    {
        private static AudioListener activeListener;

        public bool IsActive { get; private set; } = true;
        public float Volume { get; set; } = 1.0f;

        public override void Awake()
        {
            base.Awake();
            SetAsActive();
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Update(float deltaTime)
        {
            if (IsActive)
            {
                UpdateListenerPosition();
            }
        }

        public void SetAsActive()
        {
            if (activeListener != this && activeListener != null)
            {
                activeListener.Deactivate();
            }

            activeListener = this;
            IsActive = true;
        }

        public void Deactivate()
        {
            if (activeListener == this)
            {
                activeListener = null;
            }
            IsActive = false;
        }

        public static AudioListener GetActiveListener()
        {
            return activeListener;
        }

        private void UpdateListenerPosition()
        {
            if (GameObject == null || GameObject.Transform == null)
                return;

            Vector3 position = GameObject.Transform.Position;
            Vector3 forward = GameObject.Transform.Forward.Normalized();
            Vector3 up = GameObject.Transform.Up.Normalized();

            // Set position
            AL.Listener(ALListener3f.Position, position.X, position.Y, position.Z);

            // Set orientation (forward and up vector concatenated)
            float[] orientation = new float[]
            {
                forward.X, forward.Y, forward.Z,
                up.X, up.Y, up.Z
            };
            AL.Listener(ALListenerfv.Orientation, orientation); // Removed 'ref' keyword

            // Set volume (gain)
            AL.Listener(ALListenerf.Gain, Volume);

            Vector3 velocity = GameObject.GetComponent<Rigidbody>().Velocity;
            AL.Listener(ALListener3f.Velocity, velocity.X, velocity.Y, velocity.Z);
        }

        public override void OnDestroy()
        {
            if (activeListener == this)
            {
                activeListener = null;
            }
            base.OnDestroy();
        }
    }
}
