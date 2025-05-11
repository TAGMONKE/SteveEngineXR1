using OpenTK.Mathematics;
using System;

namespace SteveEngine
{
    public class Camera
    {
        private Vector3 position;
        private Vector3 front = -Vector3.UnitZ;
        private Vector3 up = Vector3.UnitY;
        private Vector3 right = Vector3.UnitX;
        
        private float yaw = -90.0f;
        private float pitch = 0.0f;
        
        private float fov = 45.0f;
        
        public float AspectRatio { get; set; }
        
        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                UpdateVectors();
            }
        }
        
        public float Yaw
        {
            get => yaw;
            set
            {
                yaw = value;
                UpdateVectors();
            }
        }
        
        public float Pitch
        {
            get => pitch;
            set
            {
                pitch = MathHelper.Clamp(value, -89.0f, 89.0f);
                UpdateVectors();
            }
        }
        
        public float Fov
        {
            get => fov;
            set
            {
                fov = MathHelper.Clamp(value, 1.0f, 90.0f);
            }
        }
        
        public Camera(Vector3 position, float width, float height)
        {
            this.position = position;
            AspectRatio = width / height;
            UpdateVectors();
        }
        
        private void UpdateVectors()
        {
            front.X = MathF.Cos(MathHelper.DegreesToRadians(pitch)) * MathF.Cos(MathHelper.DegreesToRadians(yaw));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
            front.Z = MathF.Cos(MathHelper.DegreesToRadians(pitch)) * MathF.Sin(MathHelper.DegreesToRadians(yaw));
            
            front = Vector3.Normalize(front);
            
            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }
        
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(position, position + front, up);
        }
        
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), AspectRatio, 0.01f, 100.0f);
        }
        
        public void MoveForward(float distance)
        {
            position += front * distance;
        }
        
        public void MoveRight(float distance)
        {
            position += right * distance;
        }
        
        public void MoveUp(float distance)
        {
            position += up * distance;
        }
        
        public void Update(float deltaTime)
        {
            // Camera logic to be implemented or overridden via Lua
        }
    }
}