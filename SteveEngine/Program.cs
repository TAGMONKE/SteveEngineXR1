using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft;
using OpenTK.Windowing.Common;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SteveEngine
{
    public class Program
    {
        public class Config
        {
            public string WindowTitle { get; set; } = "SteveEngine";
            public int WindowWidth { get; set; } = 800;
            public int WindowHeight { get; set; } = 600;
            public WindowState State { get; set; } = WindowState.Normal;
            public string CameraPosition { get; set; } = "0, 0, 0";
            public float CameraFov { get; set; } = 70;
            public float CameraYaw { get; set; } = -90.0f;
            public float CameraPitch { get; set; } = 0;
            public bool DebugMode = true;
            public bool isXR = false;
            public bool VSync = false;
        }

        public static Engine engine;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        const int SW_SHOW = 5;

        public static bool forceXR = false;

        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {

                Console.WriteLine("Starting SteveEngine...");

                // Load configuration
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                if (!File.Exists(configPath))
                {
                    Console.WriteLine($"Configuration file not found at {configPath}");
                    Console.WriteLine("Creating default configuration...");
                    Config defaultConfig = new Config();
                    string defaultConfigJson = Newtonsoft.Json.JsonConvert.SerializeObject(defaultConfig, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(configPath, defaultConfigJson);
                    Console.WriteLine($"Default configuration created at {configPath}");
                    Console.WriteLine("Please edit the configuration file and restart the application.");
                    return;
                }
                string configJson = File.ReadAllText(configPath);
                Config config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(configJson);
                if (config == null)
                {
                    Console.WriteLine("Failed to load configuration.");
                    return;
                }
                Console.WriteLine($"Loaded configuration: {config.WindowTitle}, {config.WindowWidth}x{config.WindowHeight}");

                engine = new SteveEngine.Engine(StrToV3(config.CameraPosition), config.WindowWidth, config.WindowHeight, config.WindowTitle, config.State, config.isXR || forceXR, false, false, false);
                Console.WriteLine("Engine created successfully");

                if(!config.DebugMode)
                {
                    var handle = GetConsoleWindow();
                    ShowWindow(handle, SW_HIDE);
                }


                // Call this after engine.LoadScript(scriptPath);
                RegisterLuaShaderLoader(engine);
                // Create shader files
                Console.WriteLine("Creating shader files...");
                var defaultShaderVert = @"
                    #version 330 core
                    layout (location = 0) in vec3 aPosition;
                    layout (location = 1) in vec3 aNormal;
                    layout (location = 2) in vec2 aTexCoord;
                    
                    out vec3 FragPos;
                    out vec3 Normal;
                    out vec2 TexCoord;
                    
                    uniform mat4 model;
                    uniform mat4 view;
                    uniform mat4 projection;
                    
                    void main()
                    {
                        gl_Position = projection * view * model * vec4(aPosition, 1.0);
                        FragPos = vec3(model * vec4(aPosition, 1.0));
                        Normal = mat3(transpose(inverse(model))) * aNormal;
                        TexCoord = aTexCoord;
                    }
                ";

                var defaultShaderFrag = @"
    #version 330 core
    out vec4 FragColor;
    
    in vec3 FragPos;
    in vec3 Normal;
    in vec2 TexCoord;
    
    uniform sampler2D diffuseTexture;
    uniform sampler2D normalMap;
    uniform vec3 lightPos = vec3(1.0, 2.0, 3.0);
    uniform vec3 lightColor = vec3(1.0, 1.0, 1.0);
    uniform vec3 viewPos = vec3(0.0, 0.0, 3.0);

    // Function to get normal from normal map
    vec3 GetNormalFromMap()
    {
        vec3 tangentNormal = texture(normalMap, TexCoord).rgb * 2.0 - 1.0;

        // Assume model matrix has no non-uniform scale for simplicity
        vec3 Q1 = dFdx(FragPos);
        vec3 Q2 = dFdy(FragPos);
        vec2 st1 = dFdx(TexCoord);
        vec2 st2 = dFdy(TexCoord);

        vec3 N = normalize(Normal);
        vec3 T = normalize(Q1 * st2.y - Q2 * st1.y);
        vec3 B = normalize(cross(N, T));
        mat3 TBN = mat3(T, B, N);

        return normalize(TBN * tangentNormal);
    }
    
    void main()
    {
        // Ambient
        float ambientStrength = 0.1;
        vec3 ambient = ambientStrength * lightColor;
        
        // Normal mapping
        vec3 norm = normalize(Normal);
        if(texture(normalMap, TexCoord).a > 0.0)
            norm = GetNormalFromMap();

        // Diffuse
        vec3 lightDir = normalize(lightPos - FragPos);
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = diff * lightColor;
        
        // Specular
        float specularStrength = 1;
        vec3 viewDir = normalize(viewPos - FragPos);
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
        vec3 specular = specularStrength * spec * lightColor;
        
        vec3 result = (ambient + diffuse + specular) * texture(diffuseTexture, TexCoord).rgb;
        FragColor = vec4(result, 1.0);
    }
";
                // Write shaders to temporary files
                string tempPath = Path.GetTempPath();
                string vertPath = Path.Combine(tempPath, "default.vert");
                string fragPath = Path.Combine(tempPath, "default.frag");

                File.WriteAllText(vertPath, defaultShaderVert);
                File.WriteAllText(fragPath, defaultShaderFrag);

                Console.WriteLine("Loading default shader...");
                // Load shader
                var shader = engine.LoadShader("default", vertPath, fragPath);
                if (shader != null)
                {
                    Console.WriteLine("Default shader loaded successfully");
                }
                else
                {
                    Console.WriteLine("Failed to load default shader");
                }

                // Create a default white texture for testing
                Console.WriteLine("Creating default texture...");
                string defaultTexturePath = Path.Combine(tempPath, "default.png");
                using (var bitmap = new Bitmap(256, 256))
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            // Create a checkerboard pattern
                            if ((x / 32 + y / 32) % 2 == 0)
                                bitmap.SetPixel(x, y, Color.White);
                            else
                                bitmap.SetPixel(x, y, Color.LightGray);
                        }
                    }

                    bitmap.Save(defaultTexturePath, ImageFormat.Png);
                }
                Console.WriteLine($"Default texture created at {defaultTexturePath}");

                // create a default normal map for testing
                Console.WriteLine("Creating default normal map...");
                string defaultNormalMapPath = Path.Combine(tempPath, "default_normal.png");
                using (var sourceTexture = new Bitmap(defaultTexturePath))
                using (var normalMap = new Bitmap(sourceTexture.Width, sourceTexture.Height))
                {
                    // Strength of the normal map effect
                    float strength = 2.0f;

                    for (int x = 0; x < normalMap.Width; x++)
                    {
                        for (int y = 0; y < normalMap.Height; y++)
                        {
                            // Sample neighboring pixels
                            int left = Math.Max(0, x - 1);
                            int right = Math.Min(sourceTexture.Width - 1, x + 1);
                            int top = Math.Max(0, y - 1);
                            int bottom = Math.Min(sourceTexture.Height - 1, y + 1);

                            // Get grayscale values of neighboring pixels
                            float leftVal = sourceTexture.GetPixel(left, y).GetBrightness();
                            float rightVal = sourceTexture.GetPixel(right, y).GetBrightness();
                            float topVal = sourceTexture.GetPixel(x, top).GetBrightness();
                            float bottomVal = sourceTexture.GetPixel(x, bottom).GetBrightness();

                            // Calculate normal based on gradient
                            float dx = (rightVal - leftVal) * strength;
                            float dy = (bottomVal - topVal) * strength;

                            // Calculate normal vector (x, y, z)
                            Vector3 normal = Vector3.Normalize(new Vector3(-dx, -dy, 1.0f));

                            // Convert from [-1,1] range to [0,1] range for RGB
                            Vector3 rgb = normal * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);

                            // Convert to color (0-255)
                            int r = (int)(rgb.X * 255);
                            int g = (int)(rgb.Y * 255);
                            int b = (int)(rgb.Z * 255);

                            normalMap.SetPixel(x, y, Color.FromArgb(255, r, g, b));
                        }
                    }

                    normalMap.Save(defaultNormalMapPath, ImageFormat.Png);
                }

                Console.WriteLine($"Default normal map created at {defaultNormalMapPath}");

                Console.WriteLine($"Default normal map created at {defaultNormalMapPath}");

                // Load sample script
                Console.WriteLine("Creating Lua script...");
                // Locate the script in the SteveEngine directory
                string enginePath = AppDomain.CurrentDomain.BaseDirectory;
                string scriptPath = Path.Combine(enginePath, "game.lua");

                if (!File.Exists(scriptPath))
                {
                    throw new FileNotFoundException($"Script file not found at {scriptPath}");
                }

                Console.WriteLine($"Lua script found at {scriptPath}");

                // Load the Lua script
                Console.WriteLine("Loading Lua script...");
                engine.LoadScript(scriptPath);

                // Run the engine
                Console.WriteLine("Starting game loop...");
                engine.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Critical error: {e.Message}");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        public static OpenTK.Mathematics.Vector3 StrToV3(string input)
        {
            string[] parts = input.Split(',');
            if (parts.Length != 3)
                throw new ArgumentException("Input string must contain three comma-separated values.");
            float x = float.Parse(parts[0]);
            float y = float.Parse(parts[1]);
            float z = float.Parse(parts[2]);
            return new OpenTK.Mathematics.Vector3(x, y, z);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();

        public static void RegisterLuaShaderLoader(Engine engine) => engine.luaState.RegisterFunction("load_shader", null, typeof(Program).GetMethod(nameof(LoadShaderFromLua)));
        public static void LoadShaderFromLua(string name, string vertPath, string fragPath)
        {
            if (engine == null)
                throw new InvalidOperationException("Engine not initialized.");

            if (!File.Exists(vertPath) || !File.Exists(fragPath))
                throw new FileNotFoundException("Shader file(s) not found.");

            var shader = engine.LoadShader(name, vertPath, fragPath);
            if (shader == null)
                throw new Exception("Failed to load shader.");
        }
    }
}