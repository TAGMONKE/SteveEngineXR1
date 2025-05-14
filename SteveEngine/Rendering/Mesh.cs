using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SteveEngine
{
    public class Mesh
    {
        private int vao;
        private int vbo;
        private int ebo;
        private int vertexCount;
        
        public Mesh(float[] vertices, uint[] indices)
        {
            vertexCount = indices.Length;
            
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            
            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            
            // Position attribute
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            
            // Normal attribute
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            
            // Texture coordinate attribute
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);
            
            GL.BindVertexArray(0);
        }
        
        public void Draw()
        {
            GL.BindVertexArray(vao);
            GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, vertexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
        
        public static Mesh CreateCube()
        {
            float[] vertices = {
                // Front face
                -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f, 0.0f, // 0
                 0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f, 0.0f, // 1
                 0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f, 1.0f, // 2
                -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f, 1.0f, // 3
                
                // Back face
                -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 0.0f, // 4
                 0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 0.0f, // 5
                 0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 1.0f, // 6
                -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 1.0f, // 7
                
                // Top face
                -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 1.0f, // 8
                 0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 1.0f, // 9
                 0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 0.0f, // 10
                -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 0.0f, // 11
                
                // Bottom face
                -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 0.0f, // 12
                 0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 0.0f, // 13
                 0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 1.0f, // 14
                -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 1.0f, // 15
                
                // Right face
                 0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 0.0f, // 16
                 0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 1.0f, // 17
                 0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 1.0f, // 18
                 0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f, // 19
                
                // Left face
                -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 0.0f, // 20
                -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 1.0f, // 21
                -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 1.0f, // 22
                -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 0.0f  // 23
            };
            
            uint[] indices = {
                0,  1,  2,  0,  2,  3,   // front
                4,  5,  6,  4,  6,  7,   // back
                8,  9,  10, 8,  10, 11,  // top
                12, 13, 14, 12, 14, 15,  // bottom
                16, 17, 18, 16, 18, 19,  // right
                20, 21, 22, 20, 22, 23   // left
            };
            
            return new Mesh(vertices, indices);
        }
      
        public static Mesh CreateSphere(float radius = 1.0f, int subdivisions = 16)
        {
            int stacks = subdivisions;
            int slices = subdivisions;

            var vertices = new List<float>();
            var indices = new List<uint>();
            uint nextIndex = 0;

            // Generate vertices
            for (int stack = 0; stack <= stacks; stack++)
            {
                float phi = MathF.PI * stack / stacks;
                float y = MathF.Cos(phi);
                float stackRadius = MathF.Sin(phi);

                for (int slice = 0; slice <= slices; slice++)
                {
                    float theta = 2 * MathF.PI * slice / slices;

                    // Position
                    float x = stackRadius * MathF.Sin(theta);
                    float z = stackRadius * MathF.Cos(theta);

                    // Normal (normalized position)
                    float nx = x;
                    float ny = y;
                    float nz = z;

                    // Texture coordinates
                    float u = (float)slice / slices;
                    float v = (float)stack / stacks;

                    vertices.Add(x * radius);
                    vertices.Add(y * radius);
                    vertices.Add(z * radius);
                    vertices.Add(nx);
                    vertices.Add(ny);
                    vertices.Add(nz);
                    vertices.Add(u);
                    vertices.Add(v);
                }
            }

            // Generate indices
            for (int stack = 0; stack < stacks; stack++)
            {
                for (int slice = 0; slice < slices; slice++)
                {
                    uint top = (uint)(stack * (slices + 1) + slice);
                    uint bottom = (uint)((stack + 1) * (slices + 1) + slice);

                    // First triangle
                    indices.Add(top);
                    indices.Add(bottom);
                    indices.Add(top + 1);

                    // Second triangle
                    indices.Add(top + 1);
                    indices.Add(bottom);
                    indices.Add(bottom + 1);
                }
            }

            return new Mesh(vertices.ToArray(), indices.ToArray());
        }

        public static Mesh CreateCapsule(float radius = 0.5f, float height = 2.0f, int subdivisions = 16)
        {
            int stacks = subdivisions;
            int slices = subdivisions;

            var vertices = new List<float>();
            var indices = new List<uint>();
            uint nextIndex = 0;

            float halfHeight = height * 0.5f;
            float cylinderHeight = height - 2 * radius;
            float halfCylinderHeight = cylinderHeight * 0.5f;

            // Bottom hemisphere
            for (int stack = stacks / 2; stack <= stacks; stack++)
            {
                float phi = MathF.PI * stack / stacks;
                float y = -halfCylinderHeight + MathF.Cos(phi) * radius; // Offset to bottom
                float stackRadius = MathF.Sin(phi) * radius;

                for (int slice = 0; slice <= slices; slice++)
                {
                    float theta = 2 * MathF.PI * slice / slices;

                    // Position
                    float x = stackRadius * MathF.Sin(theta);
                    float z = stackRadius * MathF.Cos(theta);

                    // Normal (normalized from sphere center)
                    float nx = MathF.Sin(phi) * MathF.Sin(theta);
                    float ny = MathF.Cos(phi);
                    float nz = MathF.Sin(phi) * MathF.Cos(theta);

                    // Texture coordinates
                    float u = (float)slice / slices;
                    float v = (float)stack / stacks;

                    vertices.Add(x);
                    vertices.Add(y);
                    vertices.Add(z);
                    vertices.Add(nx);
                    vertices.Add(ny);
                    vertices.Add(nz);
                    vertices.Add(u);
                    vertices.Add(v);
                }
            }

            // Cylinder
            for (int stack = 0; stack <= 1; stack++)
            {
                float y = -halfCylinderHeight + stack * cylinderHeight;

                for (int slice = 0; slice <= slices; slice++)
                {
                    float theta = 2 * MathF.PI * slice / slices;

                    // Position
                    float x = radius * MathF.Sin(theta);
                    float z = radius * MathF.Cos(theta);

                    // Normal
                    float nx = MathF.Sin(theta);
                    float ny = 0;
                    float nz = MathF.Cos(theta);

                    // Texture coordinates
                    float u = (float)slice / slices;
                    float v = (float)stack;

                    vertices.Add(x);
                    vertices.Add(y);
                    vertices.Add(z);
                    vertices.Add(nx);
                    vertices.Add(ny);
                    vertices.Add(nz);
                    vertices.Add(u);
                    vertices.Add(v);
                }
            }

            // Top hemisphere
            for (int stack = 0; stack <= stacks / 2; stack++)
            {
                float phi = MathF.PI * stack / stacks;
                float y = halfCylinderHeight + MathF.Cos(phi) * radius; // Offset to top
                float stackRadius = MathF.Sin(phi) * radius;

                for (int slice = 0; slice <= slices; slice++)
                {
                    float theta = 2 * MathF.PI * slice / slices;

                    // Position
                    float x = stackRadius * MathF.Sin(theta);
                    float z = stackRadius * MathF.Cos(theta);

                    // Normal (normalized from sphere center)
                    float nx = MathF.Sin(phi) * MathF.Sin(theta);
                    float ny = MathF.Cos(phi);
                    float nz = MathF.Sin(phi) * MathF.Cos(theta);

                    // Texture coordinates
                    float u = (float)slice / slices;
                    float v = (float)stack / stacks + 0.5f;

                    vertices.Add(x);
                    vertices.Add(y);
                    vertices.Add(z);
                    vertices.Add(nx);
                    vertices.Add(ny);
                    vertices.Add(nz);
                    vertices.Add(u);
                    vertices.Add(v);
                }
            }

            // Generate indices
            int bottomHemiStart = 0;
            int cylinderStart = (stacks / 2 + 1) * (slices + 1);
            int topHemiStart = cylinderStart + 2 * (slices + 1);

            // Bottom hemisphere
            for (int stack = 0; stack < stacks / 2; stack++)
            {
                for (int slice = 0; slice < slices; slice++)
                {
                    uint top = (uint)(bottomHemiStart + stack * (slices + 1) + slice);
                    uint bottom = (uint)(bottomHemiStart + (stack + 1) * (slices + 1) + slice);

                    indices.Add(top);
                    indices.Add(bottom);
                    indices.Add(top + 1);

                    indices.Add(top + 1);
                    indices.Add(bottom);
                    indices.Add(bottom + 1);
                }
            }

            // Cylinder
            for (int stack = 0; stack < 1; stack++)
            {
                for (int slice = 0; slice < slices; slice++)
                {
                    uint top = (uint)(cylinderStart + stack * (slices + 1) + slice);
                    uint bottom = (uint)(cylinderStart + (stack + 1) * (slices + 1) + slice);

                    indices.Add(top);
                    indices.Add(bottom);
                    indices.Add(top + 1);

                    indices.Add(top + 1);
                    indices.Add(bottom);
                    indices.Add(bottom + 1);
                }
            }

            // Top hemisphere
            for (int stack = 0; stack < stacks / 2; stack++)
            {
                for (int slice = 0; slice < slices; slice++)
                {
                    uint top = (uint)(topHemiStart + stack * (slices + 1) + slice);
                    uint bottom = (uint)(topHemiStart + (stack + 1) * (slices + 1) + slice);

                    indices.Add(top);
                    indices.Add(bottom);
                    indices.Add(top + 1);

                    indices.Add(top + 1);
                    indices.Add(bottom);
                    indices.Add(bottom + 1);
                }
            }

            return new Mesh(vertices.ToArray(), indices.ToArray());
        }

        public static Mesh CreateCone(float radius = 1.0f, float height = 2.0f, int subdivisions = 16)
        {
            int slices = subdivisions;

            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();
            uint nextIndex = 0;

            // Add the apex vertex at the top of the cone
            vertices.Add(0.0f);         // Position x
            vertices.Add(height);       // Position y
            vertices.Add(0.0f);         // Position z
            vertices.Add(0.0f);         // Normal x
            vertices.Add(1.0f);         // Normal y
            vertices.Add(0.0f);         // Normal z
            vertices.Add(0.5f);         // Texture u
            vertices.Add(0.0f);         // Texture v

            // Add the vertices for the base of the cone
            for (int slice = 0; slice <= slices; slice++)
            {
                float theta = 2 * MathF.PI * slice / slices;

                float x = radius * MathF.Sin(theta);
                float z = radius * MathF.Cos(theta);

                // Compute normal for the side of the cone
                float nx = x / MathF.Sqrt(x * x + height * height);
                float ny = height / MathF.Sqrt(x * x + height * height);
                float nz = z / MathF.Sqrt(z * z + height * height);

                // Side vertex
                vertices.Add(x);                // Position x
                vertices.Add(0.0f);            // Position y
                vertices.Add(z);                // Position z
                vertices.Add(nx);               // Normal x 
                vertices.Add(ny);               // Normal y
                vertices.Add(nz);               // Normal z
                vertices.Add((float)slice / slices); // Texture u
                vertices.Add(1.0f);             // Texture v

                // Base center vertex (for bottom cap)
                if (slice == 0)
                {
                    vertices.Add(0.0f);         // Position x
                    vertices.Add(0.0f);         // Position y
                    vertices.Add(0.0f);         // Position z
                    vertices.Add(0.0f);         // Normal x
                    vertices.Add(-1.0f);        // Normal y
                    vertices.Add(0.0f);         // Normal z
                    vertices.Add(0.5f);         // Texture u
                    vertices.Add(0.5f);         // Texture v
                }

                // Base bottom vertex
                vertices.Add(x);                // Position x
                vertices.Add(0.0f);            // Position y
                vertices.Add(z);                // Position z
                vertices.Add(0.0f);             // Normal x
                vertices.Add(-1.0f);            // Normal y
                vertices.Add(0.0f);             // Normal z
                vertices.Add(0.5f + 0.5f * MathF.Sin(theta)); // Texture u
                vertices.Add(0.5f + 0.5f * MathF.Cos(theta)); // Texture v
            }

            // Side faces
            for (int slice = 0; slice < slices; slice++)
            {
                indices.Add(0); // Apex
                indices.Add((uint)(1 + slice));
                indices.Add((uint)(1 + ((slice + 1) % slices)));
            }

            // Bottom faces
            uint baseCenter = (uint)(1 + slices + 1);
            for (int slice = 0; slice < slices; slice++)
            {
                indices.Add(baseCenter);
                indices.Add((uint)(baseCenter + 1 + ((slice + 1) % slices)));
                indices.Add((uint)(baseCenter + 1 + slice));
            }

            return new Mesh(vertices.ToArray(), indices.ToArray());
        }

        public static Mesh CreatePlane(float width = 1.0f, float depth = 1.0f)
        {
            float halfWidth = width * 0.5f;
            float halfDepth = depth * 0.5f;

            float[] vertices = {
        // Position            Normal            TexCoord
        -halfWidth, 0.0f,  halfDepth,  0.0f, 1.0f, 0.0f,  0.0f, 0.0f,
         halfWidth, 0.0f,  halfDepth,  0.0f, 1.0f, 0.0f,  1.0f, 0.0f,
         halfWidth, 0.0f, -halfDepth,  0.0f, 1.0f, 0.0f,  1.0f, 1.0f,
        -halfWidth, 0.0f, -halfDepth,  0.0f, 1.0f, 0.0f,  0.0f, 1.0f
    };

            uint[] indices = {
        0, 1, 2,
        0, 2, 3
    };

            return new Mesh(vertices, indices);
        }

        public static Mesh CreateQuad(float width = 1.0f, float height = 1.0f)
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            float[] vertices = {
        // Position               Normal            TexCoord
        -halfWidth, -halfHeight, 0.0f,  0.0f, 0.0f, 1.0f,  0.0f, 0.0f,
         halfWidth, -halfHeight, 0.0f,  0.0f, 0.0f, 1.0f,  1.0f, 0.0f,
         halfWidth,  halfHeight, 0.0f,  0.0f, 0.0f, 1.0f,  1.0f, 1.0f,
        -halfWidth,  halfHeight, 0.0f,  0.0f, 0.0f, 1.0f,  0.0f, 1.0f
    };

            uint[] indices = {
        0, 1, 2,
        0, 2, 3
    };

            return new Mesh(vertices, indices);
        }
    }
}