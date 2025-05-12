using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;


namespace SteveEngine
{
    public class AudioListener
    {
        private static AudioListener instance;

        public float Volume { get; set; } = 1.0f;

        private Camera camera;

        private AudioListener(Camera camera)
        {
            this.camera = camera;
        }

        public static AudioListener GetInstance(Camera camera)
        {
            if (instance == null)
            {
                instance = new AudioListener(camera);
            }
            return instance;
        }

        public void Update()
        {
            if (camera == null)
                return;

            Vector3 position = camera.Position;
            Vector3 forward = camera.front;
            Vector3 up = camera.up;

            // Set position
            AL.Listener(ALListener3f.Position, position.X, position.Y, position.Z);

            // Set orientation (forward and up vector concatenated)
            float[] orientation = new float[]
            {
                    forward.X, forward.Y, forward.Z,
                    up.X, up.Y, up.Z
            };
            AL.Listener(ALListenerfv.Orientation, orientation);

            // Set volume (gain)
            AL.Listener(ALListenerf.Gain, Volume);

            // Velocity is not directly available from the camera, so set it to zero
            AL.Listener(ALListener3f.Velocity, 0.0f, 0.0f, 0.0f);
        }
    }
}
