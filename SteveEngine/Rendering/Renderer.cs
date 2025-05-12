using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SteveEngine
{
    public class Renderer
    {

        private int windowWidth = 800; // Default window width  
        private int windowHeight = 600; // Default window height  
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

        public void RenderXR(List<GameObject> gameObjects, Matrix4 leftViewMatrix, Matrix4 leftProjectionMatrix, Matrix4 rightViewMatrix, Matrix4 rightProjectionMatrix)
        {
            GL.Enable(EnableCap.ScissorTest);
            GL.Viewport(0, 0, windowWidth / 2, windowHeight);
            GL.Scissor(0, 0, windowWidth / 2, windowHeight);
            foreach (var obj in gameObjects)
            {
                if (obj.Renderer == null) continue;

                var modelMatrix = obj.Transform.GetModelMatrix();
                obj.Renderer.Render(modelMatrix, leftViewMatrix, leftProjectionMatrix);
            }

            GL.Viewport(windowWidth / 2, 0, windowWidth / 2, windowHeight);
            GL.Scissor(windowWidth / 2, 0, windowWidth / 2, windowHeight);
            foreach (var obj in gameObjects)
            {
                if (obj.Renderer == null) continue;

                var modelMatrix = obj.Transform.GetModelMatrix();
                obj.Renderer.Render(modelMatrix, rightViewMatrix, rightProjectionMatrix);
            }

            GL.Viewport(0, 0, windowWidth, windowHeight);
            GL.Disable(EnableCap.ScissorTest);
        }
    }

    public class MeshRenderer : Component
    {
        public Mesh Mesh { get; set; }
        public Material Material { get; set; }

        public void LoadAssetsFromBundle(ResourceManager resourceManager, string bundlePath, string meshName, string materialName)
        {
            Mesh = resourceManager.GetAssetFromBundle<Mesh>(bundlePath, meshName);
            Material = resourceManager.GetAssetFromBundle<Material>(bundlePath, materialName);
        }

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