// Create a new ComponentRegistry.cs file
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SteveEngine
{
    public static class ComponentRegistry
    {
        private static Dictionary<string, Type> componentTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        
        public static void Initialize()
        {
            // Auto-discover and register components
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(Component)) && !type.IsAbstract)
                    {
                        RegisterComponent(type);
                    }
                }
            }
        }
        
        public static void RegisterComponent(Type componentType)
        {
            string name = componentType.Name;
            componentTypes[name] = componentType;
            Console.WriteLine($"Registered component: {name}");
        }
        
        public static Type GetComponentType(string name)
        {
            if (componentTypes.TryGetValue(name, out Type type))
                return type;
            return null;
        }
        
        public static IEnumerable<string> GetRegisteredComponentNames()
        {
            return componentTypes.Keys;
        }
    }
}
