using System;
using System.Collections.Generic;
using NLua;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using Silk.NET.OpenXR;
using Silk.NET.Core.Contexts;
using System.Collections;
using static OpenTK.Graphics.OpenGL.GL;

namespace SteveEngine
{
    public class Engine
    {
        private GameWindow window;
        private Camera camera;
        public Lua luaState;
        private List<GameObject> gameObjects = new List<GameObject>();
        private Renderer renderer;
        private ResourceManager resourceManager;
        // Add this to the member variables in Engine class
        private InputManager inputManager;
        public List<GameObject> GameObjects => gameObjects;

        public LuaGameObjectList LuaGameObjects
        {
            get { return new LuaGameObjectList(gameObjects); }
        }

        // Add this property to expose the InputManager
        public InputManager Input => inputManager;

        public bool XREnabled = false;

        public bool bp;
        public bool wp;
        public Camera Camera => camera;

        public Sun sun;

        public Engine(Vector3 cameraPosition, int width = 800, int height = 600, string title = "SteveEngine", WindowState state = WindowState.Normal, bool isXR = false, bool basePlate = false, bool wasdPlayer = false, bool VSync = false)
        {
            var nativeWindowSettings = new NativeWindowSettings
            {
                Size = new Vector2i(width, height),
                Title = title,
                WindowState = state,
                Vsync = VSync ? VSyncMode.On : VSyncMode.Off,
            };
            sun = Sun.Instance;
            window = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);
            camera = new Camera(cameraPosition, width, height);
            AudioListener.GetInstance(camera);
            renderer = new Renderer();
            resourceManager = new ResourceManager();
            
            InitializeLua();
            SetupEvents();

            if (isXR)
            {

            }
            bp = basePlate;
            wp = wasdPlayer;
        }

        private void InitializeLua()
        {
            try
            {
                luaState = new Lua();
                luaState["engine"] = this;

                // Register Lua bindings
                LuaBindings.SetupLuaBindings(luaState);

                luaState.DoString(@"
            function onUpdate(deltaTime)
                -- Will be overridden by user scripts
            end
            
            function onStart()
                -- Will be overridden by user scripts
            end
            
            -- Test that Lua is working
            print('Lua environment initialized successfully')
        ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Lua: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void SetupEvents()
        {
            window.Load += OnLoad;
            window.UpdateFrame += OnUpdateFrame;
            window.RenderFrame += OnRenderFrame;
            window.Resize += OnResize;
            inputManager = new InputManager(window);

            window.KeyDown += OnKeyDown;
            window.KeyUp += OnKeyUp;
            window.MouseDown += OnMouseDown;
            window.MouseUp += OnMouseUp;
            window.MouseMove += OnMouseMove;
        }

        private void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            Console.WriteLine("Engine OnLoad called, running onStart from Lua");

            try
            {
                LuaFunction startFunc = luaState.GetFunction("onStart");
                if (startFunc != null)
                {
                    startFunc.Call();
                    Console.WriteLine("Lua onStart function completed");
                }
                else
                {
                    Console.WriteLine("Lua onStart function not found");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in Lua onStart: {e.Message}");
                if (e.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {e.InnerException.Message}");
                    Console.WriteLine(e.InnerException.StackTrace);
                }
            }

            // Print information about created game objects
            Console.WriteLine($"Game objects after onStart: {gameObjects.Count}");
            for (int i = 0; i < gameObjects.Count; i++)
            {
                Console.WriteLine($"  Object {i}: {gameObjects[i].Name}");
            }


            if (bp)
            {
                // spawn baseplate
                var baseplate = CreateGameObject("Baseplate");
                var meshRenderer = baseplate.AddComponent("MeshRenderer");
                meshRenderer.GameObject.Transform.Scale = new Vector3(100, 1, 100);
            }

            if (wp)
            {
                // spawn player
                var player = CreateGameObject("Player");
                var meshRenderer = player.AddComponent("CharacterController");
                meshRenderer.GameObject.Transform.Scale = new Vector3(1, 2, 1);
                player.AddComponent("PlayerController");
            }
        }

        private void OnUpdateFrame(FrameEventArgs e)
        {
            float deltaTime = (float)e.Time;

            // Update time system
            Time.Update(deltaTime);

            // Accumulate time for physics updates
            physicsAccumulator += deltaTime;

            // Run fixed timestep physics updates
            while (physicsAccumulator >= FIXED_TIME_STEP)
            {
                Physics.FixedUpdate();
                physicsAccumulator -= FIXED_TIME_STEP;
            }

            try
            {
                LuaFunction updateFunc = luaState.GetFunction("onUpdate");
                if (updateFunc != null)
                {
                    updateFunc.Call(e.Time);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Lua onUpdate: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }

            foreach (var obj in gameObjects)
            {
                obj.Update((float)e.Time);
            }

            if (camera != null)
            {
                camera.Update((float)e.Time);
            }
        }

        private float physicsAccumulator = 0f;
        private const float FIXED_TIME_STEP = 0.02f;

        private void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            renderer.Render(gameObjects, camera);
            
            window.SwapBuffers();
        }
        
        private void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
        }
        
        public void Run()
        {
            window.Run();
        }
        
        public GameObject CreateGameObject(string name)
        {
            var gameObject = new GameObject(name);
            gameObjects.Add(gameObject);
            return gameObject;
        }
        
        public void LoadScript(string path)
        {
            try
            {
                luaState.DoFile(path);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading script {path}: {e.Message}");
            }
        }
        
        public Material CreateMaterial(string shaderName)
        {
            return resourceManager.CreateMaterial(shaderName);
        }
        
        public Texture LoadTexture(string path)
        {
            return resourceManager.LoadTexture(path);
        }
        
        public Shader LoadShader(string name, string vertexPath, string fragmentPath)
        {
            return resourceManager.LoadShader(name, vertexPath, fragmentPath);
        }

        // Add these methods to the Engine class
        private void OnKeyDown(KeyboardKeyEventArgs e)
        {
            // Additional event handling if needed
        }

        private void OnKeyUp(KeyboardKeyEventArgs e)
        {
            // Additional event handling if needed
        }

        private void OnMouseDown(MouseButtonEventArgs e)
        {
            // Additional event handling if needed
        }

        private void OnMouseUp(MouseButtonEventArgs e)
        {
            // Additional event handling if needed
        }

        private void OnMouseMove(MouseMoveEventArgs e)
        {
            // Additional event handling if needed
        }
    }
}