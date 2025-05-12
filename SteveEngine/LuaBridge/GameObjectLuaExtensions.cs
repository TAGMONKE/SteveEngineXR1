using System;
using OpenTK.Mathematics;

namespace SteveEngine
{
    // Extension methods for GameObject to support Lua interop
    public static class GameObjectLuaExtensions
    {
        public static void SetPosition(this GameObject gameObject, float x, float y, float z)
        {
            if (gameObject?.Transform == null)
            {
                Console.WriteLine("GameObject or Transform not available");
                return;
            }
            
            gameObject.Transform.Position = new Vector3(x, y, z);
        }

        public static Vector3 GetPosition(this GameObject gameObject)
        {
            if (gameObject?.Transform == null)
            {
                Console.WriteLine("GameObject or Transform not available");
                return Vector3.Zero;
            }
            
            return gameObject.Transform.Position;
        }

        public static void MoveBy(this GameObject gameObject, Vector3 offset)
        {
            gameObject.Transform.Position += offset;
        }

        public static void AddForce(this GameObject gameObject, Vector3 force, ForceMode mode = ForceMode.Force)
        {
            var rb = gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(force, mode);
            }
        }

        public static void LookAt(this GameObject gameObject, Vector3 target)
        {
            // Implementation to rotate object toward target
        }

        public static void SetRotation(this GameObject gameObject, float x, float y, float z)
        {
            if (gameObject?.Transform == null)
            {
                Console.WriteLine("GameObject or Transform not available");
                return;
            }
            
            gameObject.Transform.Rotation = new Vector3(x, y, z);
        }

        public static Vector3 GetRotation(this GameObject gameObject)
        {
            if (gameObject?.Transform == null)
            {
                Console.WriteLine("GameObject or Transform not available");
                return Vector3.Zero;
            }
            
            return gameObject.Transform.Rotation;
        }

        public static void SetScale(this GameObject gameObject, float x, float y, float z)
        {
            if (gameObject?.Transform == null)
            {
                Console.WriteLine("GameObject or Transform not available");
                return;
            }
            
            gameObject.Transform.Scale = new Vector3(x, y, z);
        }

        public static Vector3 GetScale(this GameObject gameObject)
        {
            if (gameObject?.Transform == null)
            {
                Console.WriteLine("GameObject or Transform not available");
                return Vector3.One;
            }
            
            return gameObject.Transform.Scale;
        }
    }
}
