using System;
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

            Console.WriteLine("Lua bindings setup complete");

            // Test the bindings
            try
            {
                lua.DoString(@"
                    print('Testing Vector3:', Vector3(1, 2, 3).X, Vector3(1, 2, 3).Y, Vector3(1, 2, 3).Z)
                    print('Mesh table exists:', Mesh ~= nil)
                    print('Mesh.CreateCube exists:', type(Mesh.CreateCube) == 'function')
                ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing Lua bindings: {ex.Message}");
            }
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