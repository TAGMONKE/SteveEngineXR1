using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace SteveEngine
{
    public enum ForceMode
    {
        Force,            
        Acceleration,       
        Impulse,          
        VelocityChange        
    }

    public class Rigidbody : Component
    {
        public float Mass { get; set; } = 1.0f;
        public Vector3 Velocity { get; set; } = Vector3.Zero;
        public Vector3 AngularVelocity { get; set; } = Vector3.Zero;
        public float Drag { get; set; } = 0.05f;   
        public float AngularDrag { get; set; } = 0.05f;   
        public bool UseGravity { get; set; } = true;
        public bool IsKinematic { get; set; } = false;        
        public bool FreezeRotation { get; set; } = false;      

        public static Vector3 Gravity = new Vector3(0, -9.81f, 0);

        private Vector3 accumulatedForce = Vector3.Zero;
        private Vector3 accumulatedTorque = Vector3.Zero;

        private List<Collider> collidingWith = new List<Collider>();

        public Rigidbody()
        {
        }

        public override void Update(float deltaTime)
        {
            if (IsKinematic)
                return;       

            if (GameObject?.Transform == null)
                return;

            if (UseGravity)
            {
                AddForce(Gravity * Mass, ForceMode.Force);
            }

            ApplyAccumulatedForces(deltaTime);

            ApplyDrag(deltaTime);

            GameObject.Transform.Position += Velocity * deltaTime;

            if (!FreezeRotation)
            {
                Vector3 rotationChange = AngularVelocity * deltaTime;
                GameObject.Transform.Rotation += rotationChange;
            }

            ClearAccumulatedForces();

            HandleCollisions();
        }

        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            switch (mode)
            {
                case ForceMode.Force:
                    accumulatedForce += force;        
                    break;
                case ForceMode.Acceleration:
                    accumulatedForce += force * Mass;       
                    break;
                case ForceMode.Impulse:
                    Velocity += force / Mass;         
                    break;
                case ForceMode.VelocityChange:
                    Velocity += force;      
                    break;
            }
        }

        public void AddForceXYZ(float x, float y, float z, ForceMode mode = ForceMode.Force)
        {
            Vector3 force = Program.StrToV3($"{x},{y},{z}");

            switch (mode)
            {
                case ForceMode.Force:
                    accumulatedForce += force;        
                    break;
                case ForceMode.Acceleration:
                    accumulatedForce += force * Mass;       
                    break;
                case ForceMode.Impulse:
                    Velocity += force / Mass;         
                    break;
                case ForceMode.VelocityChange:
                    Velocity += force;      
                    break;
            }
        }

        public void AddRelativeForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            Vector3 worldForce = TransformDirectionToWorld(force);
            AddForce(worldForce, mode);
        }

        public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force)
        {
            switch (mode)
            {
                case ForceMode.Force:
                    accumulatedTorque += torque;      
                    break;
                case ForceMode.Acceleration:
                    accumulatedTorque += torque * Mass;
                    break;
                case ForceMode.Impulse:
                    AngularVelocity += torque / Mass;
                    break;
                case ForceMode.VelocityChange:
                    AngularVelocity += torque;
                    break;
            }
        }

        public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force)
        {
            AddForce(force, mode);

            Vector3 relativePos = position - GameObject.Transform.Position;
            Vector3 torque = Vector3.Cross(relativePos, force);
            AddTorque(torque, mode);
        }

        public bool IsCollidingWith(Collider other)
        {
            return collidingWith.Contains(other);
        }

        public void AddCollision(Collider collider)
        {
            if (!collidingWith.Contains(collider))
            {
                collidingWith.Add(collider);
                OnCollisionEnter(collider);
            }
        }

        public void RemoveCollision(Collider collider)
        {
            if (collidingWith.Contains(collider))
            {
                collidingWith.Remove(collider);
                OnCollisionExit(collider);
            }
        }

        protected virtual void OnCollisionEnter(Collider collider)
        {
            Console.WriteLine($"Collision started between {GameObject.Name} and {collider.GameObject.Name}");
        }

        protected virtual void OnCollisionExit(Collider collider)
        {
            Console.WriteLine($"Collision ended between {GameObject.Name} and {collider.GameObject.Name}");
        }

        private void ApplyAccumulatedForces(float deltaTime)
        {
            Vector3 acceleration = accumulatedForce / Mass;
            Velocity += acceleration * deltaTime;

            Vector3 angularAcceleration = accumulatedTorque / Mass;
            AngularVelocity += angularAcceleration * deltaTime;
        }

        private void ApplyDrag(float deltaTime)
        {
            Velocity *= MathF.Pow(1.0f - Drag, deltaTime);

            if (!FreezeRotation)
            {
                AngularVelocity *= MathF.Pow(1.0f - AngularDrag, deltaTime);

                const float epsilon = 0.001f;
                if (AngularVelocity.LengthSquared < epsilon * epsilon)
                {
                    AngularVelocity = Vector3.Zero;
                }
            }
        }

        private void ClearAccumulatedForces()
        {
            accumulatedForce = Vector3.Zero;
            accumulatedTorque = Vector3.Zero;
        }

        private void HandleCollisions()
        {
            if (IsKinematic)
                return;

            List<Collider> attachedColliders = new List<Collider>();
            foreach (var component in GameObject.Components)
            {
                if (component is Collider collider)
                {
                    attachedColliders.Add(collider);
                }
            }

            if (attachedColliders.Count == 0)
                return;

            foreach (var collider in attachedColliders)
            {
                if (collider.IsTrigger)
                    continue;

                foreach (var otherCollider in collidingWith)
                {
                    if (otherCollider.IsTrigger)
                        continue;

                    if (otherCollider.AttachedRigidbody != null)
                    {
                        collider.ResolveCollision(otherCollider, Time.DeltaTime);
                    }
                }
            }
        }

        private Vector3 TransformDirectionToWorld(Vector3 localDirection)
        {
            if (GameObject?.Transform == null)
                return localDirection;

            Quaternion rotation = Quaternion.FromEulerAngles(
                MathHelper.DegreesToRadians(GameObject.Transform.Rotation.X),
                MathHelper.DegreesToRadians(GameObject.Transform.Rotation.Y),
                MathHelper.DegreesToRadians(GameObject.Transform.Rotation.Z));

            Quaternion rotated = rotation * new Quaternion(localDirection.X, localDirection.Y, localDirection.Z, 0) *
                                  Quaternion.Conjugate(rotation);

            return new Vector3(rotated.X, rotated.Y, rotated.Z);
        }

    }
}
