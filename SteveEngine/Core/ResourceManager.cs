using System;
using System.Collections.Generic;
using System.IO;

namespace SteveEngine
{
    public class ResourceManager
    {
        private Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
        private Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        public Shader LoadShader(string name, string vertexPath, string fragmentPath)
        {
            try
            {
                if (shaders.ContainsKey(name)) return shaders[name];

                Console.WriteLine($"Loading shader {name} from {vertexPath} and {fragmentPath}");
                if (!File.Exists(vertexPath))
                {
                    Console.WriteLine($"Vertex shader file not found: {vertexPath}");
                    return null;
                }

                if (!File.Exists(fragmentPath))
                {
                    Console.WriteLine($"Fragment shader file not found: {fragmentPath}");
                    return null;
                }

                string vertexCode = File.ReadAllText(vertexPath);
                string fragmentCode = File.ReadAllText(fragmentPath);

                Shader shader = new Shader(vertexCode, fragmentCode);
                shaders[name] = shader;
                Console.WriteLine($"Shader {name} loaded successfully");

                return shader;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading shader {name}: {e.Message}");
                return null;
            }
        }

        public Texture LoadTexture(string path)
        {
            try
            {
                if (textures.ContainsKey(path)) return textures[path];

                Console.WriteLine($"Loading texture from {path}");
                if (!File.Exists(path))
                {
                    Console.WriteLine($"Texture file not found: {path}");
                    return null;
                }

                Texture texture = new Texture(path);
                textures[path] = texture;
                Console.WriteLine($"Texture loaded successfully");

                return texture;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading texture {path}: {e.Message}");
                return null;
            }
        }

        public Material CreateMaterial(string shaderName)
        {
            try
            {
                if (!shaders.ContainsKey(shaderName))
                {
                    Console.WriteLine($"Shader {shaderName} not found");
                    return null;
                }

                Console.WriteLine($"Creating material with shader {shaderName}");
                return new Material(shaders[shaderName]);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating material with shader {shaderName}: {e.Message}");
                return null;
            }
        }
    }
}