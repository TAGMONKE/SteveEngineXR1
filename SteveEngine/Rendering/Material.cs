using System.Collections.Generic;
using OpenTK.Mathematics;

namespace SteveEngine
{
    public class Material
    {
        public Shader Shader { get; set; }
        public Dictionary<string, Texture> Textures { get; private set; }
        
        public Material(Shader shader)
        {
            Shader = shader;
            Textures = new Dictionary<string, Texture>();
        }
        
        public void SetTexture(string name, Texture texture)
        {
            if (Textures.ContainsKey(name))
            {
                Textures[name] = texture;
            }
            else
            {
                Textures.Add(name, texture);
            }
        }
        
        public void SetVector3(string name, Vector3 value)
        {
            Shader.SetVector3(name, value);
        }
        
        public void SetFloat(string name, float value)
        {
            Shader.SetFloat(name, value);
        }
        
        public void SetMatrix4(string name, Matrix4 value)
        {
            Shader.SetMatrix4(name, value);
        }
        
        public void Use()
        {
            Shader.Use();
            
            int textureUnit = 0;
            foreach (var texture in Textures)
            {
                texture.Value.Use(textureUnit);
                Shader.SetInt(texture.Key, textureUnit);
                textureUnit++;
            }
        }
    }
}