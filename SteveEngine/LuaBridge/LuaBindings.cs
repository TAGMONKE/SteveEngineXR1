using System;
using System.Collections.Generic;
using NLua;
using OpenTK.Mathematics;

namespace SteveEngine
{
    public static class LuaBindings
    {
        public static void SetupLuaBindings(Lua lua)
        {
            Console.WriteLine("Setting up Lua bindings...");

            // Register Vector3 constructor
            lua.DoString(@"
                    function Vector3(x, y, z)
                        x = x or 0
                        y = y or 0
                        z = z or 0
                        return {X = x, Y = y, Z = z}
                    end
                ");

            // Create a factory for Mesh instead of exposing the class directly
            lua.DoString(@"
                    Mesh = {}
                ");

            // Register the MeshFactory to handle Mesh creation
            lua["MeshFactory"] = new MeshFactory();
            lua.DoString(@"
                    function Mesh.CreateCube()
                        return MeshFactory:CreateCube()
                    end
                ");

            // Register helper functions for game object access
            lua.DoString(@"
                    function GetGameObject(index)
                        if not engine or not engine.LuaGameObjects then
                            print('Engine or LuaGameObjects not available')
                            return nil
                        end
                        return engine.LuaGameObjects:Get(index)
                    end
                    
                    function GetGameObjectCount()
                        if not engine or not engine.LuaGameObjects then
                            return 0
                        end
                        return engine.LuaGameObjects:Count()
                    end
                ");

            // Register camera access
            lua.DoString(@"
                    Camera = {}

                    function Camera.GetPosition()
                        if not engine or not engine.Camera then
                            print('Engine or Camera not available')
                            return nil
                        end
                        local pos = engine.Camera:GetPosition()
                        return {X = pos.X, Y = pos.Y, Z = pos.Z}
                    end

                    function Camera.SetPosition(x, y, z)
                        if not engine or not engine.Camera then
                            print('Engine or Camera not available')
                            return
                        end
                        engine.Camera:SetPosition(Vector3(x, y, z))
                    end

                    function Camera.GetRotation()
                        if not engine or not engine.Camera then
                            print('Engine or Camera not available')
                            return nil
                        end
                        local rot = engine.Camera:GetRotation()
                        return {X = rot.X, Y = rot.Y, Z = rot.Z}
                    end

                    function Camera.SetRotation(x, y, z)
                        if not engine or not engine.Camera then
                            print('Engine or Camera not available')
                            return
                        end
                        engine.Camera:SetRotation(Vector3(x, y, z))
                    end
                ");

            Console.WriteLine("Lua bindings setup complete");

            // Test the bindings
            try
            {
                lua.DoString(@"
                        print('Testing Vector3:', Vector3(1, 2, 3).X, Vector3(1, 2, 3).Y, Vector3(1, 2, 3).Z)
                        print('Mesh table exists:', Mesh ~= nil)
                        print('Mesh.CreateCube exists:', type(Mesh.CreateCube) == 'function')
                        print('Camera table exists:', Camera ~= nil)
                    ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing Lua bindings: {ex.Message}");
            }

            lua.DoString(@"
            Input = {}
            
            -- Keyboard input functions
            function Input.IsKeyPressed(key)
                return engine.Input:IsKeyPressed(key)
            end
            
            function Input.IsKeyDown(key)
                return engine.Input:IsKeyDown(key)
            end
            
            function Input.IsKeyReleased(key)
                return engine.Input:IsKeyReleased(key)
            end
            
            -- Mouse input functions
            function Input.IsMouseButtonPressed(button)
                return engine.Input:IsMouseButtonPressed(button)
            end
            
            function Input.IsMouseButtonDown(button)
                return engine.Input:IsMouseButtonDown(button)
            end
            
            function Input.IsMouseButtonReleased(button)
                return engine.Input:IsMouseButtonReleased(button)
            end
            
            function Input.GetMousePosition()
                return engine.Input:GetMousePosition()
            end
            
            function Input.GetMouseDelta()
                return engine.Input:GetMouseDelta()
            end
        ");

            // Register key and mouse button enums for Lua
            RegisterKeyEnums(lua);

            // Test the Input bindings
            lua.DoString(@"
            print('Input table exists:', Input ~= nil)
            print('Input functions available:', 
                  type(Input.IsKeyDown) == 'function',
                  type(Input.GetMousePosition) == 'function')
        ");
        }

        // Add this method to the LuaBindings class
        private static void RegisterKeyEnums(Lua lua)
        {
            // Create tables for key codes and mouse buttons
            lua.DoString(@"
            Keys = {}
            MouseButton = {}
        ");

            // Register common keyboard keys
            var keyValues = new Dictionary<string, OpenTK.Windowing.GraphicsLibraryFramework.Keys>
        {
            {"A", OpenTK.Windowing.GraphicsLibraryFramework.Keys.A},
            {"B", OpenTK.Windowing.GraphicsLibraryFramework.Keys.B},
            {"C", OpenTK.Windowing.GraphicsLibraryFramework.Keys.C},
            {"D", OpenTK.Windowing.GraphicsLibraryFramework.Keys.D},
            {"E", OpenTK.Windowing.GraphicsLibraryFramework.Keys.E},
            {"F", OpenTK.Windowing.GraphicsLibraryFramework.Keys.F},
            {"G", OpenTK.Windowing.GraphicsLibraryFramework.Keys.G},
            {"H", OpenTK.Windowing.GraphicsLibraryFramework.Keys.H},
            {"I", OpenTK.Windowing.GraphicsLibraryFramework.Keys.I},
            {"J", OpenTK.Windowing.GraphicsLibraryFramework.Keys.J},
            {"K", OpenTK.Windowing.GraphicsLibraryFramework.Keys.K},
            {"L", OpenTK.Windowing.GraphicsLibraryFramework.Keys.L},
            {"M", OpenTK.Windowing.GraphicsLibraryFramework.Keys.M},
            {"N", OpenTK.Windowing.GraphicsLibraryFramework.Keys.N},
            {"O", OpenTK.Windowing.GraphicsLibraryFramework.Keys.O},
            {"P", OpenTK.Windowing.GraphicsLibraryFramework.Keys.P},
            {"Q", OpenTK.Windowing.GraphicsLibraryFramework.Keys.Q},
            {"R", OpenTK.Windowing.GraphicsLibraryFramework.Keys.R},
            {"S", OpenTK.Windowing.GraphicsLibraryFramework.Keys.S},
            {"T", OpenTK.Windowing.GraphicsLibraryFramework.Keys.T},
            {"U", OpenTK.Windowing.GraphicsLibraryFramework.Keys.U},
            {"V", OpenTK.Windowing.GraphicsLibraryFramework.Keys.V},
            {"W", OpenTK.Windowing.GraphicsLibraryFramework.Keys.W},
            {"X", OpenTK.Windowing.GraphicsLibraryFramework.Keys.X},
            {"Y", OpenTK.Windowing.GraphicsLibraryFramework.Keys.Y},
            {"Z", OpenTK.Windowing.GraphicsLibraryFramework.Keys.Z},
            {"Space", OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space},
            {"Escape", OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape},
            {"Enter", OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter},
            {"Up", OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up},
            {"Down", OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down},
            {"Left", OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left},
            {"Right", OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right},
            {"LeftShift", OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift},
            {"RightShift", OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightShift},
            {"LeftControl", OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftControl},
            {"RightControl", OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightControl}
        };

            foreach (var kvp in keyValues)
            {
                lua[$"Keys.{kvp.Key}"] = kvp.Value;
            }

            // Register mouse buttons
            lua["MouseButton.Left"] = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left;
            lua["MouseButton.Right"] = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right;
            lua["MouseButton.Middle"] = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle;
        }
    }

    // Factory class to create Mesh objects for Lua
    public class MeshFactory
    {
        public Mesh CreateCube()
        {
            Console.WriteLine("MeshFactory: Creating cube mesh");
            return Mesh.CreateCube();
        }
    }
}