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
        }
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        const int SW_SHOW = 5;

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

                var engine = new SteveEngine.Engine(StrToV3(config.CameraPosition), config.WindowWidth, config.WindowHeight, config.WindowTitle, config.State);
                Console.WriteLine("Engine created successfully");

                if(!config.DebugMode)
                {
                    var handle = GetConsoleWindow();
                    ShowWindow(handle, SW_HIDE);
                }

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
                    uniform vec3 lightPos = vec3(1.0, 2.0, 3.0);
                    uniform vec3 lightColor = vec3(1.0, 1.0, 1.0);
                    uniform vec3 viewPos = vec3(0.0, 0.0, 3.0);
                    
                    void main()
                    {
                        // Ambient
                        float ambientStrength = 0.1;
                        vec3 ambient = ambientStrength * lightColor;
                        
                        // Diffuse
                        vec3 norm = normalize(Normal);
                        vec3 lightDir = normalize(lightPos - FragPos);
                        float diff = max(dot(norm, lightDir), 0.0);
                        vec3 diffuse = diff * lightColor;
                        
                        // Specular
                        float specularStrength = 0.5;
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
    }
}