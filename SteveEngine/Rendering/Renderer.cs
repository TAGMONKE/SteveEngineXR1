using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SteveEngine
{
    public class Renderer
    {
        private int windowWidth = 800;
        private int windowHeight = 600;
        private Fence renderFence = new Fence();

        public void Render(List<GameObject> gameObjects, Camera camera)
        {
            // Wait for previous frame to finish if still rendering
            if (!renderFence.IsSignaled())
            {
                renderFence.WaitUntilSignaled();
            }

            var viewMatrix = camera.GetViewMatrix();
            var projectionMatrix = camera.GetProjectionMatrix();

            // 1. Group by Material (and optionally Mesh)
            var batches = new Dictionary<Material, List<(MeshRenderer, Matrix4)>>();

            foreach (var obj in gameObjects)
            {
                if (obj.Renderer is MeshRenderer mr && mr.Material != null && mr.Mesh != null)
                {
                    if (!batches.TryGetValue(mr.Material, out var list))
                    {
                        list = new List<(MeshRenderer, Matrix4)>();
                        batches[mr.Material] = list;
                    }
                    list.Add((mr, obj.Transform.GetModelMatrix()));
                }
            }

            // 2. Render by batch (minimize state changes)
            foreach (var kv in batches)
            {
                var material = kv.Key;
                var renderers = kv.Value;

                material.Use(); // Bind shader, set textures, etc.

                foreach (var (mr, modelMatrix) in renderers)
                {
                    mr.Render(modelMatrix, viewMatrix, projectionMatrix);
                }
            }

            // Insert fence to track when this frame's rendering completes
            renderFence.Insert();
        }

        public void RenderXR(List<GameObject> gameObjects, Matrix4 leftViewMatrix, Matrix4 leftProjectionMatrix, Matrix4 rightViewMatrix, Matrix4 rightProjectionMatrix)
        {
            // Wait for previous frame to complete if still rendering
            if (!renderFence.IsSignaled())
            {
                renderFence.WaitUntilSignaled();
            }

            GL.Enable(EnableCap.ScissorTest);
            GL.Viewport(0, 0, windowWidth / 2, windowHeight);
            GL.Scissor(0, 0, windowWidth / 2, windowHeight);

            RenderEye(gameObjects, leftViewMatrix, leftProjectionMatrix);

            GL.Viewport(windowWidth / 2, 0, windowWidth / 2, windowHeight);
            GL.Scissor(windowWidth / 2, 0, windowWidth / 2, windowHeight);

            RenderEye(gameObjects, rightViewMatrix, rightProjectionMatrix);

            GL.Viewport(0, 0, windowWidth, windowHeight);
            GL.Disable(EnableCap.ScissorTest);

            // Insert fence to track when this frame's rendering completes
            renderFence.Insert();
        }

        private void RenderEye(List<GameObject> gameObjects, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            var batches = new Dictionary<Material, List<(MeshRenderer, Matrix4)>>();

            foreach (var obj in gameObjects)
            {
                if (obj.Renderer is MeshRenderer mr && mr.Material != null && mr.Mesh != null)
                {
                    if (!batches.TryGetValue(mr.Material, out var list))
                    {
                        list = new List<(MeshRenderer, Matrix4)>();
                        batches[mr.Material] = list;
                    }
                    list.Add((mr, obj.Transform.GetModelMatrix()));
                }
            }

            foreach (var kv in batches)
            {
                var material = kv.Key;
                var renderers = kv.Value;

                material.Use();

                foreach (var (mr, modelMatrix) in renderers)
                {
                    mr.Render(modelMatrix, viewMatrix, projectionMatrix);
                }
            }
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