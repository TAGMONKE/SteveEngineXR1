using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace SteveEngine
{
    public class GameObject
    {
        public string Name { get; set; }
        public Transform Transform { get; private set; }
        public MeshRenderer Renderer { get; set; }
        public List<Component> Components { get; private set; }
        
        public GameObject(string name)
        {
            Name = name;
            Transform = new Transform();
            Components = new List<Component>();
        }
        
        public void Update(float deltaTime)
        {
            foreach (var component in Components)
            {
                component.Update(deltaTime);
            }
        }

        public Component AddComponent(string componentTypeName)
        {
            try
            {
                // Look for the component type in the current assembly
                Type componentType = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    componentType = assembly.GetType($"SteveEngine.{componentTypeName}");
                    if (componentType != null)
                        break;
                }

                if (componentType == null || !typeof(Component).IsAssignableFrom(componentType))
                {
                    Console.WriteLine($"Component type {componentTypeName} not found or not a Component");
                    return null;
                }

                Component component = (Component)Activator.CreateInstance(componentType);
                component.GameObject = this;
                Components.Add(component);

                // Special case for MeshRenderer
                if (componentTypeName == "MeshRenderer")
                {
                    Renderer = (MeshRenderer)component;
                }

                return component;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error adding component {componentTypeName}: {e.Message}");
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        public T GetComponent<T>() where T : Component
        {
            foreach (var component in Components)
            {
                if (component is T typedComponent)
                    return typedComponent;
            }
            return null;
        }

        public bool HasComponent<T>() where T : Component
        {
            return GetComponent<T>() != null;
        }

    }

    public class Transform
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;

        public Vector3 Forward => Vector3.Normalize(new Vector3(
            MathF.Cos(Rotation.Y) * MathF.Cos(Rotation.X),
            MathF.Sin(Rotation.X),
            MathF.Sin(Rotation.Y) * MathF.Cos(Rotation.X)
        ));

        public Vector3 Right => Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));

        public Vector3 Up => Vector3.Normalize(Vector3.Cross(Right, Forward));

        public Matrix4 GetModelMatrix()
        {
            var model = Matrix4.CreateScale(Scale);
            model *= Matrix4.CreateRotationX(Rotation.X);
            model *= Matrix4.CreateRotationY(Rotation.Y);
            model *= Matrix4.CreateRotationZ(Rotation.Z);
            model *= Matrix4.CreateTranslation(Position);
            return model;
        }
    }
}