using System;
using System.Collections.Generic;
using NLua;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;

namespace SteveEngine
{
    public class Engine
    {
        private GameWindow window;
        private Camera camera;
        private Lua luaState;
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

        public Engine(int width = 800, int height = 600, string title = "SteveEngine")
        {
            var nativeWindowSettings = new NativeWindowSettings
            {
                Size = new Vector2i(width, height),
                Title = title
            };

            window = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);
            camera = new Camera(Vector3.Zero, width, height);
            renderer = new Renderer();
            resourceManager = new ResourceManager();
            
            InitializeLua();
            SetupEvents();
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
            inputManager = new InputManager();

            window.KeyDown += OnKeyDown;
            window.KeyUp += OnKeyUp;
            window.MouseDown += OnMouseDown;
            window.MouseUp += OnMouseUp;
            window.MouseMove += OnMouseMove;

            inputManager.Update(window.KeyboardState, window.MouseState);

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
        }

        private void OnUpdateFrame(FrameEventArgs e)
        {
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

            camera.Update((float)e.Time);
        }

        private void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            renderer.Render(gameObjects, camera);
            
            window.SwapBuffers();
        }
        
        private void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            camera.AspectRatio = e.Width / (float)e.Height;
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