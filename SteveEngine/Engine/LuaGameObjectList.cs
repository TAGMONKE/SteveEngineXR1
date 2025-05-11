using System;
using System.Collections.Generic;

namespace SteveEngine
{
    public class LuaGameObjectList
    {
        private readonly List<GameObject> gameObjects;

        public LuaGameObjectList(List<GameObject> gameObjects)
        {
            this.gameObjects = gameObjects;
        }

        public GameObject Get(int index)
        {
            try
            {
                // Lua uses 1-based indexing, C# uses 0-based indexing
                int csharpIndex = index - 1;

                if (csharpIndex >= 0 && csharpIndex < gameObjects.Count)
                {
                    return gameObjects[csharpIndex];
                }

                Console.WriteLine($"Index out of range: {index} (Lua) / {csharpIndex} (C#), Count: {gameObjects.Count}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing game object at index {index}: {ex.Message}");
                return null;
            }
        }

        public int Count()
        {
            return gameObjects.Count;
        }
    }
}