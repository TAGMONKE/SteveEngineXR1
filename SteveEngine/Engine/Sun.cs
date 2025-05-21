using System;
using OpenTK.Mathematics;

namespace SteveEngine
{
    /// <summary>
    /// Represents a directional light source (sun) in the scene
    /// </summary>
    public class Sun
    {
        private Vector3 direction;
        private Vector3 color;
        private float intensity;
        private static Sun instance;
        private Shader beamShader;

        // Sun beams effect
        private DynamicBeams sunBeams;

        /// <summary>
        /// Gets or sets the sun's direction vector
        /// </summary>
        public Vector3 Direction
        {
            get => direction;
            set
            {
                direction = Vector3.Normalize(value);
                OnSunChanged();
            }
        }

        /// <summary>
        /// Gets or sets the sun's color
        /// </summary>
        public Vector3 Color
        {
            get => color;
            set
            {
                color = value;
                OnSunChanged();
            }
        }

        /// <summary>
        /// Gets or sets the sun's intensity (brightness)
        /// </summary>
        public float Intensity
        {
            get => intensity;
            set
            {
                intensity = MathHelper.Clamp(value, 0.0f, 10.0f);
                OnSunChanged();
            }
        }

        /// <summary>
        /// Gets the current ambient light level based on sun direction
        /// </summary>
        public float AmbientLevel
        {
            get
            {
                // Calculate ambient light based on sun direction (higher when sun is up)
                return MathHelper.Clamp(0.1f + 0.3f * -Math.Min(direction.Y, 0), 0.1f, 0.4f);
            }
        }

        /// <summary>
        /// Gets the singleton instance of the Sun
        /// </summary>
        public static Sun Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Sun();
                }
                return instance;
            }
        }

        /// <summary>
        /// Sets the shader to be used by the instance
        /// </summary>
        /// <param name="shader">The shader to use for sun beams</param>
        public static void SetShader(Shader shader)
        {
            if (instance != null)
            {
                instance.beamShader = shader;
                instance.InitializeSunBeams();
            }
        }

        /// <summary>
        /// Gets the sun beams effect instance
        /// </summary>
        public DynamicBeams SunBeams => sunBeams;

        /// <summary>
        /// Creates a new Sun with default settings
        /// </summary>
        public Sun()
        {
            // Default to light shining from above and slightly to the side
            direction = Vector3.Normalize(new Vector3(0.5f, -1.0f, 0.3f));
            color = new Vector3(1.0f, 0.95f, 0.9f); // Slightly warm light
            intensity = 1.0f;
            // Note: sunBeams will be initialized when beamShader is set
        }

        /// <summary>
        /// Creates a sun with specific direction, color, and intensity
        /// </summary>
        public Sun(Vector3 direction, Vector3 color, float intensity)
        {
            this.direction = Vector3.Normalize(direction);
            this.color = color;
            this.intensity = MathHelper.Clamp(intensity, 0.0f, 10.0f);
            // Note: sunBeams will be initialized when beamShader is set
        }

        /// <summary>
        /// Creates a sun with specific direction, color, intensity, and shader
        /// </summary>
        public Sun(Vector3 direction, Vector3 color, float intensity, Shader shader)
        {
            this.direction = Vector3.Normalize(direction);
            this.color = color;
            this.intensity = MathHelper.Clamp(intensity, 0.0f, 10.0f);
            this.beamShader = shader;
            InitializeSunBeams();
        }

        /// <summary>
        /// Sets up the sun's position based on pitch and yaw angles
        /// </summary>
        /// <param name="pitch">Vertical angle in degrees (0 = horizon, 90 = directly overhead)</param>
        /// <param name="yaw">Horizontal angle in degrees (0 = North, 90 = East)</param>
        public void SetAngles(float pitch, float yaw)
        {
            // Convert angles to radians
            float pitchRad = MathHelper.DegreesToRadians(pitch);
            float yawRad = MathHelper.DegreesToRadians(yaw);

            // Calculate direction vector from angles
            Vector3 newDirection = new Vector3(
                MathF.Cos(pitchRad) * MathF.Sin(yawRad),
                -MathF.Sin(pitchRad),
                MathF.Cos(pitchRad) * MathF.Cos(yawRad)
            );

            Direction = Vector3.Normalize(newDirection);
        }

        /// <summary>
        /// Simulates day/night cycle by setting sun position based on time
        /// </summary>
        /// <param name="timeOfDay">Time between 0 and 24 (hours)</param>
        public void SetTimeOfDay(float timeOfDay)
        {
            // Normalize time to the range [0, 24]
            timeOfDay = timeOfDay % 24;

            // Convert time to pitch angle (0 at dawn/dusk, 90 at noon, -90 at midnight)
            float pitch = 90 * MathF.Sin((timeOfDay - 6) / 12 * MathF.PI);

            // For simplicity, keep yaw constant (could be modified for seasons)
            float yaw = 0;

            SetAngles(pitch, yaw);

            // Adjust color based on time (warm at sunrise/sunset, normal at midday, blue at night)
            if (timeOfDay > 5 && timeOfDay < 8) // Sunrise
            {
                Color = new Vector3(1.0f, 0.8f, 0.6f);
                if (sunBeams != null)
                {
                    sunBeams.isBeamsActive = true;
                    sunBeams.beamCount = 6;
                }
            }
            else if (timeOfDay > 8 && timeOfDay < 17) // Midday
            {
                Color = new Vector3(1.0f, 0.95f, 0.9f);
                if (sunBeams != null)
                {
                    sunBeams.isBeamsActive = true;
                    sunBeams.beamCount = sunBeams.maxBeamCount;
                }
            }
            else if (timeOfDay > 17 && timeOfDay < 20) // Sunset
            {
                Color = new Vector3(1.0f, 0.6f, 0.4f);
                if (sunBeams != null)
                {
                    sunBeams.isBeamsActive = true;
                    sunBeams.beamCount = 6;
                }
            }
            else // Night
            {
                Color = new Vector3(0.2f, 0.2f, 0.5f);
                Intensity = 0.3f;
                if (sunBeams != null)
                {
                    sunBeams.isBeamsActive = false;
                    sunBeams.beamCount = 0;
                }
                return;
            }

            // Full intensity during the day
            Intensity = 1.0f;
        }

        /// <summary>
        /// Updates shaders with the sun's properties
        /// </summary>
        public void ApplyToShader(Shader shader)
        {
            if (shader == null) return;

            // Set light direction
            shader.SetVector3("lightDir", -direction); // Negative because light comes FROM this direction

            // Set light color (taking intensity into account)
            shader.SetVector3("lightColor", color * intensity);

            // Set ambient lighting
            shader.SetFloat("ambientStrength", AmbientLevel);

            // Optionally, set sun beams parameters if needed in shader
            if (sunBeams != null && sunBeams.isBeamsActive)
            {
                shader.SetInt("beamCount", sunBeams.beamCount);
                shader.SetFloat("beamLength", sunBeams.beamLength);
                shader.SetFloat("beamWidth", sunBeams.beamWidth);
            }
            else
            {
                shader.SetInt("beamCount", 0);
            }
        }

        /// <summary>
        /// Notifies the rendering system when sun properties change
        /// </summary>
        private void OnSunChanged()
        {
            // This method will be called whenever sun properties change
            // It could notify any registered listeners, or update global lighting variables
            Console.WriteLine($"Sun changed: Dir={direction}, Color={color}, Intensity={intensity}, BeamsActive={sunBeams?.isBeamsActive ?? false}");
        }

        /// <summary>
        /// Initializes sun beams with default values
        /// </summary>
        private void InitializeSunBeams()
        {
            if (beamShader == null) return;

            sunBeams = new DynamicBeams(beamShader);
            sunBeams.isBeamsActive = true;
            sunBeams.beamCount = sunBeams.maxBeamCount;
            sunBeams.beamLength = 100;
            sunBeams.beamWidth = 5;
        }
    }
}
