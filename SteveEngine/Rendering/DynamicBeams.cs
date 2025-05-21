using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;

namespace SteveEngine
{
    public class DynamicBeams
    {
        private List<Beam> beams;
        private Random random;

        public bool isBeamsActive = false;
        public int beamCount = 0;
        public int maxBeamCount = 25;
        public int beamLength = 100;
        public int beamWidth = 5;
        public float opacity = 0.5f;
        public float fadeDistance = 1000f;
        public Color beamColor = Color.FromArgb(255, 255, 220, 100); // Default to warm sunlight
        public Vector3 lightSource = Vector3.Zero;
        public float jitterAmount = 0.05f;
        public float beamSpeed = 0.2f;
        public bool useVolumetricRendering = true;
        public int lodLevels = 3;

        // OpenGL resources for a quad
        private int quadVao = -1;
        private int quadVbo = -1;
        private Shader beamShader;

        public DynamicBeams(Shader shader)
        {
            beams = new List<Beam>();
            random = new Random();
            beamShader = shader;
            SetupQuad();
            GenerateBeams();
        }

        private void SetupQuad()
        {
            // Simple quad centered at origin, facing +Z
            float[] quadVertices = {
                -0.5f, -0.5f, 0f,
                 0.5f, -0.5f, 0f,
                 0.5f,  0.5f, 0f,
                -0.5f,  0.5f, 0f
            };
            uint[] quadIndices = { 0, 1, 2, 2, 3, 0 };

            quadVao = GL.GenVertexArray();
            quadVbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();

            GL.BindVertexArray(quadVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, quadIndices.Length * sizeof(uint), quadIndices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.BindVertexArray(0);
        }

        public void GenerateBeams()
        {
            beams.Clear();
            beamCount = Math.Min(beamCount, maxBeamCount);

            for (int i = 0; i < beamCount; i++)
            {
                AddBeam();
            }
        }

        public void AddBeam()
        {
            if (beams.Count >= maxBeamCount)
                return;

            float angle = (float)(random.NextDouble() * Math.PI * 2);
            float distance = (float)(random.NextDouble() * 10f);

            Vector3 direction = new Vector3(
                (float)Math.Sin(angle) * distance,
                -1f,
                (float)Math.Cos(angle) * distance
            );
            direction = Vector3.Normalize(direction);

            Vector3 position = new Vector3(
                lightSource.X + (float)(random.NextDouble() - 0.5f) * 20f,
                lightSource.Y,
                lightSource.Z + (float)(random.NextDouble() - 0.5f) * 20f
            );

            beams.Add(new Beam
            {
                Position = position,
                Direction = direction,
                Length = beamLength + random.Next(-20, 20),
                Width = beamWidth + (float)(random.NextDouble() * 2 - 1),
                Opacity = opacity * (float)(0.7f + random.NextDouble() * 0.3f),
                Speed = beamSpeed * (float)(0.8f + random.NextDouble() * 0.4f),
                JitterOffset = 0f
            });

            beamCount = beams.Count;
        }

        public void RemoveBeam()
        {
            if (beams.Count > 0)
            {
                beams.RemoveAt(beams.Count - 1);
                beamCount = beams.Count;
            }
        }

        public void Update(float deltaTime, Vector3 cameraPosition)
        {
            if (!isBeamsActive)
                return;

            foreach (var beam in beams)
            {
                beam.JitterOffset += deltaTime * beam.Speed;
                float distanceToCamera = (beam.Position - cameraPosition).Length;
                beam.DistanceFactor = Math.Clamp(1f - (distanceToCamera / fadeDistance), 0f, 1f);
                beam.CurrentLodLevel = Math.Min(lodLevels - 1, (int)(distanceToCamera / (fadeDistance / lodLevels)));
            }
        }

        public void Render(Matrix4 view, Matrix4 projection, Vector3 cameraPosition)
        {
            if (!isBeamsActive)
                return;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            beamShader.Use();

            foreach (var beam in beams)
            {
                if (beam.DistanceFactor <= 0.01f)
                    continue;

                float xJitter = (float)Math.Sin(beam.JitterOffset * 1.7f) * jitterAmount;
                float zJitter = (float)Math.Cos(beam.JitterOffset * 2.3f) * jitterAmount;
                Vector3 jitteredPos = beam.Position + new Vector3(xJitter, 0, zJitter);

                float effectiveOpacity = beam.Opacity * beam.DistanceFactor;
                float effectiveWidth = beam.Width * (1f - (0.25f * beam.CurrentLodLevel));

                if (useVolumetricRendering)
                {
                    RenderVolumetricBeam(jitteredPos, beam.Direction, beam.Length, effectiveWidth, effectiveOpacity, beam.CurrentLodLevel, view, projection, cameraPosition);
                }
                else
                {
                    RenderSimpleBeam(jitteredPos, beam.Direction, beam.Length, effectiveWidth, effectiveOpacity, view, projection, cameraPosition);
                }
            }

            GL.Disable(EnableCap.Blend);
        }

        private void RenderVolumetricBeam(Vector3 position, Vector3 direction, float length, float width, float opacity, int lodLevel, Matrix4 view, Matrix4 projection, Vector3 cameraPosition)
        {
            int sampleCount = 15 - (lodLevel * 5);
            float stepSize = length / sampleCount;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / (sampleCount - 1);
                float segmentOpacity = opacity * (1.0f - t);
                Vector3 segmentPos = position + direction * (stepSize * i);
                DrawBillboard(segmentPos, direction, width, segmentOpacity, view, projection, cameraPosition);
            }
        }

        private void RenderSimpleBeam(Vector3 position, Vector3 direction, float length, float width, float opacity, Matrix4 view, Matrix4 projection, Vector3 cameraPosition)
        {
            DrawBillboard(position + direction * (length * 0.5f), direction, width, opacity, view, projection, cameraPosition, length);
        }

        private void DrawBillboard(Vector3 position, Vector3 direction, float width, float opacity, Matrix4 view, Matrix4 projection, Vector3 cameraPosition, float length = 1f)
        {
            // Compute billboard orientation: face camera or align to direction
            Vector3 up = Vector3.UnitY;
            Vector3 right = Vector3.Normalize(Vector3.Cross(direction, up));
            if (right.LengthSquared < 0.01f)
                right = Vector3.UnitX;
            up = Vector3.Normalize(Vector3.Cross(right, direction));

            Matrix4 model = Matrix4.Identity;
            model.Row0 = new Vector4(right * width, 0);
            model.Row1 = new Vector4(up * length, 0);
            model.Row2 = new Vector4(direction, 0);
            model.Row3 = new Vector4(position, 1);

            beamShader.SetMatrix4("model", model);
            beamShader.SetMatrix4("view", view);
            beamShader.SetMatrix4("projection", projection);

            var c = beamColor;
            beamShader.SetVector3("beamColor", new Vector3(c.R / 255f, c.G / 255f, c.B / 255f));
            beamShader.SetFloat("beamOpacity", opacity);

            GL.BindVertexArray(quadVao);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        public class Beam
        {
            public Vector3 Position { get; set; }
            public Vector3 Direction { get; set; }
            public float Length { get; set; }
            public float Width { get; set; }
            public float Opacity { get; set; }
            public float Speed { get; set; }
            public float JitterOffset { get; set; }
            public float DistanceFactor { get; set; } = 1f;
            public int CurrentLodLevel { get; set; } = 0;
        }
    }
}
