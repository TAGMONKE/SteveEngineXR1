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
            // Add this line in RegisterPhysicsWrappers method
            // The method `RegisterExtensionType` does not exist in the `NLua.Method` namespace. 
            // To fix this, you can remove the line or replace it with an appropriate alternative if needed. 
            // Since the `GameObjectLuaExtensions` methods are extension methods, they are automatically available 
            // to Lua scripts if the `GameObject` type is exposed to Lua. 

            // Remove or comment out the problematic line:
            // NLua.Method.RegisterExtensionType(typeof(GameObjectLuaExtensions));

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

            // Define GameObject table before adding methods to it
            lua.DoString(@"
        GameObject = {}
    ");

            // Register Transform access and manipulation
            lua.DoString(@"
        -- Transform property access
        function GameObject:GetPosition()
            if not self or not self.Transform then
                print('GameObject or Transform not available')
                return Vector3(0, 0, 0)
            end
            local pos = self.Transform.Position
            return {X = pos.X, Y = pos.Y, Z = pos.Z}
        end

        function GameObject:SetPosition(x, y, z)
            if not self or not self.Transform then
                print('GameObject or Transform not available')
                return
            end
            self.Transform.Position = Vector3(x, y, z)
        end

        function GameObject:GetRotation()
            if not self or not self.Transform then
                print('GameObject or Transform not available')
                return Vector3(0, 0, 0)
            end
            local rot = self.Transform.Rotation
            return {X = rot.X, Y = rot.Y, Z = rot.Z}
        end

        function GameObject:SetRotation(x, y, z)
            if not self or not self.Transform then
                print('GameObject or Transform not available')
                return
            end
            self.Transform.Rotation = Vector3(x, y, z)
        end

        function GameObject:GetScale()
            if not self or not self.Transform then
                print('GameObject or Transform not available')
                return Vector3(1, 1, 1)
            end
            local scale = self.Transform.Scale
            return {X = scale.X, Y = scale.Y, Z = scale.Z}
        end

        function GameObject:SetScale(x, y, z)
            if not self or not self.Transform then
                print('GameObject or Transform not available')
                return
            end
            self.Transform.Scale = Vector3(x, y, z)
        end
    ");

            // Register the MeshFactory to handle Mesh creation
            lua["MeshFactory"] = new MeshFactory();
            lua.DoString(@"
        function Mesh.CreateCube()
            return MeshFactory:CreateCube()
        end
    ");

            // Add this to your SetupLuaBindings method in LuaBindings.cs, just after the GameObject table definition
            lua.DoString(@"
    -- Helper functions for game object access
    function GetGameObject(index)
        if not engine or not engine.LuaGameObjects then
            print('Engine or LuaGameObjects not available')
            return nil
        end
        return engine.LuaGameObjects:Get(index)  -- Adjust for 0-based index in C#
    end
    
    function GetGameObjectCount()
        if not engine or not engine.LuaGameObjects then
            return 0
        end
        return engine.LuaGameObjects:Count()
    end
    
    function GetGameObjectByName(name)
        if not engine or not engine.LuaGameObjects then
            print('Engine or LuaGameObjects not available')
            return nil
        end
        
        local count = engine.LuaGameObjects:Count()
        for i = 1, count do
            local obj = GetGameObject(i)
            if obj and obj.Name == name then
                return obj
            end
        end
        return nil
    end
");


            // Register camera access
            lua.DoString(@"
                        Camera = {}

                        function Camera.GetCPosition()
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

                        function Camera.GetCRotation()
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

                        function Camera.MoveForward(dist)
                            if not engine or not engine.Camera then
                                print('Engine or Camera not available')
                                return
                            end
                            engine.Camera:MoveForward(dist)
                        end

                        function Camera.MoveRight(dist)
                            if not engine or not engine.Camera then
                                print('Engine or Camera not available')
                                return
                            end
                            engine.Camera:MoveRight(dist)
                        end
                        function Camera.MoveUp(dist)
                            if not engine or not engine.Camera then
                                print('Engine or Camera not available')
                                return
                            end
                            engine.Camera:MoveUp(dist)
                        end
                        function Camera.RotateRight(dist)
                            if not engine or not engine.Camera then
                                print('Engine or Camera not available')
                                return
                            end
                            engine.Camera:RotateRight(dist)
                        end
                        function Camera.RotateUp(dist)
                            if not engine or not engine.Camera then
                                print('Engine or Camera not available')
                                return
                            end
                            engine.Camera:RotateUp(dist)
                        end

                    ");

            // Register Time bindings for physics and gameplay timing
            lua.DoString(@"
                        Time = {}

                        function Time.GetDeltaTime()
                            return engine.Time.DeltaTime
                        end

                        function Time.GetTotalTime()
                            return engine.Time.TotalTime
                        end
                        
                        function Time.GetFixedDeltaTime()
                            return engine.Time.FixedDeltaTime
                        end
                        
                        function Time.SetFixedDeltaTime(value)
                            engine.Time:SetFixedDeltaTime(value)
                        end
                    ");

            // Register Rigidbody access and manipulation
            lua.DoString(@"
                        -- Add Rigidbody component functions for GameObject
                        function GameObject:AddRigidbody()
                            return self:AddComponent('Rigidbody')
                        end

                        function GameObject:GetRigidbody()
                            return self:GetComponent('Rigidbody')
                        end
                        
                        -- Rigidbody component API
                        Rigidbody = {}
                        
                        function Rigidbody:GetMass()
                            return self.Mass
                        end
                        
                        function Rigidbody:SetMass(value)
                            self.Mass = value
                        end
                        
                        function Rigidbody:GetVelocity()
                            local vel = self.Velocity
                            return {X = vel.X, Y = vel.Y, Z = vel.Z}
                        end
                        
                        function Rigidbody:SetVelocity(x, y, z)
                            self.Velocity = Vector3(x, y, z)
                        end
                        
                        function Rigidbody:GetAngularVelocity()
                            local vel = self.AngularVelocity
                            return {X = vel.X, Y = vel.Y, Z = vel.Z}
                        end
                        
                        function Rigidbody:SetAngularVelocity(x, y, z)
                            self.AngularVelocity = Vector3(x, y, z)
                        end
                        
                        function Rigidbody:GetUseGravity()
                            return self.UseGravity
                        end
                        
                        function Rigidbody:SetUseGravity(value)
                            self.UseGravity = value
                        end
                        
                        function Rigidbody:GetIsKinematic()
                            return self.IsKinematic
                        end
                        
                        function Rigidbody.SetIsKinematic(value)
                            self.IsKinematic = value
                        end
                        
                        function Rigidbody:GetDrag()
                            return self.Drag
                        end
                        
                        function Rigidbody:SetDrag(value)
                            self.Drag = value
                        end
                        
                        function Rigidbody:GetAngularDrag()
                            return self.AngularDrag
                        end
                        
                        function Rigidbody:SetAngularDrag(value)
                            self.AngularDrag = value
                        end
                        
                        function Rigidbody:GetFreezeRotation()
                            return self.FreezeRotation
                        end
                        
                        function Rigidbody:SetFreezeRotation(value)
                            self.FreezeRotation = value
                        end
                        
                        -- Force application methods
                        function Rigidbody:AddForce(x, y, z, mode)
                            mode = mode or 0  -- Default to ForceMode.Force (0)
                            self:_AddForce(Vector3(x, y, z), mode)
                        end
                        
                        function Rigidbody:AddRelativeForce(x, y, z, mode)
                            mode = mode or 0  -- Default to ForceMode.Force
                            self:_AddRelativeForce(Vector3(x, y, z), mode)
                        end
                        
                        function Rigidbody:AddTorque(x, y, z, mode)
                            mode = mode or 0  -- Default to ForceMode.Force
                            self:_AddTorque(Vector3(x, y, z), mode)
                        end
                        
                        function Rigidbody:AddForceAtPosition(forceX, forceY, forceZ, posX, posY, posZ, mode)
                            mode = mode or 0  -- Default to ForceMode.Force
                            self:_AddForceAtPosition(Vector3(forceX, forceY, forceZ), Vector3(posX, posY, posZ), mode)
                        end
                    ");

            // Register Collider access and manipulation
            lua.DoString(@"
                        -- Collider type enum
                        ColliderType = {
                            Box = 0,
                            Sphere = 1,
                            Capsule = 2
                        }
                        
                        -- ForceMode enum for applying forces
                        ForceMode = {
                            Force = 0,
                            Acceleration = 1,
                            Impulse = 2,
                            VelocityChange = 3
                        }
                        
                        -- Add Collider component functions for GameObject
                        function GameObject:AddCollider(colliderType)
                            colliderType = colliderType or ColliderType.Box
                            return self:AddColliderWithType(colliderType)
                        end
                        
                        function GameObject:GetCollider()
                            return self:GetComponent('Collider')
                        end
                        
                        -- Collider component API
                        Collider = {}
                        
                        function Collider:GetCenter()
                            local center = self.Center
                            return {X = center.X, Y = center.Y, Z = center.Z}
                        end
                        
                        function Collider:SetCenter(x, y, z)
                            self.Center = Vector3(x, y, z)
                        end
                        
                        function Collider:GetSize()
                            local size = self.Size
                            return {X = size.X, Y = size.Y, Z = size.Z}
                        end
                        
                        function Collider:SetSize(x, y, z)
                            self.Size = Vector3(x, y, z)
                        end
                        
                        function Collider:GetRadius()
                            return self.Radius
                        end
                        
                        function Collider:SetRadius(value)
                            self.Radius = value
                        end
                        
                        function Collider:GetHeight()
                            return self.Height
                        end
                        
                        function Collider:SetHeight(value)
                            self.Height = value
                        end
                        
                        function Collider:GetIsTrigger()
                            return self.IsTrigger
                        end
                        
                        function Collider:SetIsTrigger(value)
                            self.IsTrigger = value
                        end
                        
                        function Collider:GetType()
                            return self.Type
                        end
                        
                        function Collider:GetAttachedRigidbody()
                            return self.AttachedRigidbody
                        end
                        
                        function Collider:Intersects(otherCollider)
                            return self:_Intersects(otherCollider)
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
                            print('Time table exists:', Time ~= nil)
                            print('Rigidbody table exists:', Rigidbody ~= nil)
                            print('Collider table exists:', Collider ~= nil)
                            print('ColliderType enum exists:', ColliderType ~= nil)
                            print('ForceMode enum exists:', ForceMode ~= nil)
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

            // Register Rigidbody and Collider wrapper classes
            RegisterPhysicsWrappers(lua);

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

        // Register wrapper methods for physics component interaction
        private static void RegisterPhysicsWrappers(Lua lua)
        {
            // Time wrapper class
            lua["LuaTimeWrapper"] = new LuaTimeWrapper();

            // GameObject extension methods
            lua.DoString(@"
                    function GameObject:AddColliderWithType(colliderType)
                        return engine.LuaPhysicsWrapper:AddColliderToGameObject(self, colliderType)
                    end

                    function GameObject:AddComponent(componentType)
                        return engine.LuaPhysicsWrapper:AddComponentToGameObject(self, componentType)
                    end
                    
                    function GameObject:GetComponent(componentType)
                        return engine.LuaPhysicsWrapper:GetComponentFromGameObject(self, componentType)
                    end
                ");

            // Register rigidbody wrapper
            lua["engine.LuaPhysicsWrapper"] = new LuaPhysicsWrapper();

            // Register extension methods for Rigidbody
            lua.DoString(@"
                    function Rigidbody:_AddForce(vector, mode)
                        engine.LuaPhysicsWrapper:AddForceToRigidbody(self, vector, mode)
                    end
                    
                    function Rigidbody:_AddRelativeForce(vector, mode)
                        engine.LuaPhysicsWrapper:AddRelativeForceToRigidbody(self, vector, mode)
                    end
                    
                    function Rigidbody:_AddTorque(vector, mode)
                        engine.LuaPhysicsWrapper:AddTorqueToRigidbody(self, vector, mode)
                    end
                    
                    function Rigidbody:_AddForceAtPosition(force, position, mode)
                        engine.LuaPhysicsWrapper:AddForceAtPositionToRigidbody(self, force, position, mode)
                    end
                ");

            // Register extension methods for Collider
            lua.DoString(@"
                    function Collider:_Intersects(other)
                        return engine.LuaPhysicsWrapper:ColliderIntersects(self, other)
                    end
                ");
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

    // Wrapper class for Time static class
    public class LuaTimeWrapper
    {
        public void SetFixedDeltaTime(float value)
        {
            Time.FixedDeltaTime = value;
        }
    }

    // Wrapper class for physics components
    public class LuaPhysicsWrapper
    {
        // GameObject component management
        public Component AddComponentToGameObject(GameObject gameObject, string componentType)
        {
            if (gameObject == null)
                return null;

            // Handle known component types
            switch (componentType.ToLower())
            {
                case "rigidbody":
                    var rb = new Rigidbody();
                    rb.GameObject = gameObject;
                    gameObject.Components.Add(rb);
                    return rb;

                default:
                    Console.WriteLine($"Unknown component type: {componentType}");
                    return null;
            }
        }

        public Component GetComponentFromGameObject(GameObject gameObject, string componentType)
        {
            if (gameObject == null)
                return null;

            foreach (var component in gameObject.Components)
            {
                if (componentType.ToLower() == "rigidbody" && component is Rigidbody)
                    return component;
                else if (componentType.ToLower() == "collider" && component is Collider)
                    return component;
            }

            return null;
        }

        public Collider AddColliderToGameObject(GameObject gameObject, int colliderType)
        {
            if (gameObject == null)
                return null;

            var collider = new Collider((ColliderType)colliderType);
            collider.GameObject = gameObject;
            gameObject.Components.Add(collider);
            return collider;
        }

        // Rigidbody force methods
        public void AddForceToRigidbody(Rigidbody rigidbody, dynamic vector, int forceMode)
        {
            if (rigidbody == null || vector == null)
                return;

            Vector3 force = new Vector3(
                Convert.ToSingle(vector.X),
                Convert.ToSingle(vector.Y),
                Convert.ToSingle(vector.Z));

            rigidbody.AddForce(force, (ForceMode)forceMode);
        }

        public void AddRelativeForceToRigidbody(Rigidbody rigidbody, dynamic vector, int forceMode)
        {
            if (rigidbody == null || vector == null)
                return;

            Vector3 force = new Vector3(
                Convert.ToSingle(vector.X),
                Convert.ToSingle(vector.Y),
                Convert.ToSingle(vector.Z));

            rigidbody.AddRelativeForce(force, (ForceMode)forceMode);
        }

        public void AddTorqueToRigidbody(Rigidbody rigidbody, dynamic vector, int forceMode)
        {
            if (rigidbody == null || vector == null)
                return;

            Vector3 torque = new Vector3(
                Convert.ToSingle(vector.X),
                Convert.ToSingle(vector.Y),
                Convert.ToSingle(vector.Z));

            rigidbody.AddTorque(torque, (ForceMode)forceMode);
        }

        public void AddForceAtPositionToRigidbody(Rigidbody rigidbody, dynamic force, dynamic position, int forceMode)
        {
            if (rigidbody == null || force == null || position == null)
                return;

            Vector3 forceVec = new Vector3(
                Convert.ToSingle(force.X),
                Convert.ToSingle(force.Y),
                Convert.ToSingle(force.Z));

            Vector3 posVec = new Vector3(
                Convert.ToSingle(position.X),
                Convert.ToSingle(position.Y),
                Convert.ToSingle(position.Z));

            rigidbody.AddForceAtPosition(forceVec, posVec, (ForceMode)forceMode);
        }

        // Collider methods
        public bool ColliderIntersects(Collider collider, Collider other)
        {
            if (collider == null || other == null)
                return false;

            return collider.Intersects(other);
        }
    }
}
