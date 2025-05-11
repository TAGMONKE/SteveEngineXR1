using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SteveEngine
{
    public class Renderer
    {
        public void Render(List<GameObject> gameObjects, Camera camera)
        {
            var viewMatrix = camera.GetViewMatrix();
            var projectionMatrix = camera.GetProjectionMatrix();
            
            foreach (var obj in gameObjects)
            {
                if (obj.Renderer == null) continue;
                
                var modelMatrix = obj.Transform.GetModelMatrix();
                obj.Renderer.Render(modelMatrix, viewMatrix, projectionMatrix);
            }
        }
    }
    
    public class MeshRenderer : Component
    {
        public Mesh Mesh { get; set; }
        public Material Material { get; set; }
        
        public void Render(Matrix4 modelMatrix, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            if (Mesh == null || Material == null) return;
            
            Material.Use();
            Material.SetMatrix4("model", modelMatrix);
            Material.SetMatrix4("view", viewMatrix);
            Material.SetMatrix4("projection", projectionMatrix);
            
            Mesh.Draw();
        }
    }
}