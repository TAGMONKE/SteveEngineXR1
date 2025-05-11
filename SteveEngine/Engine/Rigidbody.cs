using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace SteveEngine
{
    // Force application modes
    public enum ForceMode
    {
        Force,      // Apply a continuous force, using mass
        Acceleration, // Apply a continuous acceleration, ignoring mass
        Impulse,    // Apply an instant force, using mass
        VelocityChange // Apply an instant velocity change, ignoring mass
    }

    public class Rigidbody : Component
    {
        // Physics properties
        public float Mass { get; set; } = 1.0f;
        public Vector3 Velocity { get; set; } = Vector3.Zero;
        public Vector3 AngularVelocity { get; set; } = Vector3.Zero;
        public float Drag { get; set; } = 0.05f; // Air resistance
        public float AngularDrag { get; set; } = 0.05f; // Rotational resistance
        public bool UseGravity { get; set; } = true;
        public bool IsKinematic { get; set; } = false; // If true, physics won't move the object
        public bool FreezeRotation { get; set; } = false; // If true, object won't rotate

        // The gravity applied to all rigidbodies
        public static Vector3 Gravity = new Vector3(0, -9.81f, 0);

        // Keep track of accumulated forces to apply during the next physics update
        private Vector3 accumulatedForce = Vector3.Zero;
        private Vector3 accumulatedTorque = Vector3.Zero;

        // Last collision information
        private List<Collider> collidingWith = new List<Collider>();

        public Rigidbody()
        {
            // Default constructor
        }

        public override void Update(float deltaTime)
        {
            if (IsKinematic)
                return; // Kinematic objects don't move with physics

            // Apply physics only if we have a transform
            if (GameObject?.Transform == null)
                return;

            // Apply gravity
            if (UseGravity)
            {
                AddForce(Gravity * Mass, ForceMode.Force);
            }

            // Apply accumulated forces
            ApplyAccumulatedForces(deltaTime);

            // Apply drag
            ApplyDrag(deltaTime);

            // Update position based on velocity
            GameObject.Transform.Position += Velocity * deltaTime;

            // Update rotation based on angular velocity (if not frozen)
            if (!FreezeRotation)
            {
                // Convert angular velocity to Euler angles change
                Vector3 rotationChange = AngularVelocity * deltaTime;
                GameObject.Transform.Rotation += rotationChange;
            }

            // Reset forces for next frame
            ClearAccumulatedForces();

            // Handle collisions (this would be more complex in a real physics system)
            HandleCollisions();
        }

        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            switch (mode)
            {
                case ForceMode.Force:
                    accumulatedForce += force; // Force will be divided by mass later
                    break;
                case ForceMode.Acceleration:
                    accumulatedForce += force * Mass; // Apply as force but ignore mass
                    break;
                case ForceMode.Impulse:
                    Velocity += force / Mass; // Impulse causes instant velocity change based on mass
                    break;
                case ForceMode.VelocityChange:
                    Velocity += force; // Instant velocity change ignoring mass
                    break;
            }
        }

        public void AddForceXYZ(float x, float y, float z, ForceMode mode = ForceMode.Force)
        {
            Vector3 force = Program.StrToV3($"{x},{y},{z}");

            switch (mode)
            {
                case ForceMode.Force:
                    accumulatedForce += force; // Force will be divided by mass later
                    break;
                case ForceMode.Acceleration:
                    accumulatedForce += force * Mass; // Apply as force but ignore mass
                    break;
                case ForceMode.Impulse:
                    Velocity += force / Mass; // Impulse causes instant velocity change based on mass
                    break;
                case ForceMode.VelocityChange:
                    Velocity += force; // Instant velocity change ignoring mass
                    break;
            }
        }

        public void AddRelativeForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            // Convert local space force to world space
            // This is simplified and would need proper transformation based on rotation
            Vector3 worldForce = TransformDirectionToWorld(force);
            AddForce(worldForce, mode);
        }

        public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force)
        {
            switch (mode)
            {
                case ForceMode.Force:
                    accumulatedTorque += torque; // Torque will affect angular acceleration
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

        // Apply a force at a specific position, which may create torque
        public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force)
        {
            // Add the direct force
            AddForce(force, mode);

            // Calculate torque (cross product of position relative to center of mass and force)
            Vector3 relativePos = position - GameObject.Transform.Position;
            Vector3 torque = Vector3.Cross(relativePos, force);
            AddTorque(torque, mode);
        }

        // Method to check if a collision is occurring with another collider
        public bool IsCollidingWith(Collider other)
        {
            return collidingWith.Contains(other);
        }

        // Add a collider to the list of colliding objects
        public void AddCollision(Collider collider)
        {
            if (!collidingWith.Contains(collider))
            {
                collidingWith.Add(collider);
                OnCollisionEnter(collider);
            }
        }

        // Remove a collider from the list of colliding objects
        public void RemoveCollision(Collider collider)
        {
            if (collidingWith.Contains(collider))
            {
                collidingWith.Remove(collider);
                OnCollisionExit(collider);
            }
        }

        // Event method called when a new collision occurs
        protected virtual void OnCollisionEnter(Collider collider)
        {
            // Override this in derived classes or call an event
            Console.WriteLine($"Collision started between {GameObject.Name} and {collider.GameObject.Name}");
        }

        // Event method called when a collision ends
        protected virtual void OnCollisionExit(Collider collider)
        {
            // Override this in derived classes or call an event
            Console.WriteLine($"Collision ended between {GameObject.Name} and {collider.GameObject.Name}");
        }

        // Helper methods
        private void ApplyAccumulatedForces(float deltaTime)
        {
            // Apply force: F = ma, so a = F/m
            Vector3 acceleration = accumulatedForce / Mass;
            Velocity += acceleration * deltaTime;

            // Apply torque (simplified)
            // In a real system, this would involve moment of inertia
            Vector3 angularAcceleration = accumulatedTorque / Mass;
            AngularVelocity += angularAcceleration * deltaTime;
        }

        private void ApplyDrag(float deltaTime)
        {
            // Linear drag
            Velocity *= MathF.Pow(1.0f - Drag, deltaTime);

            // Angular drag
            if (!FreezeRotation)
            {
                AngularVelocity *= MathF.Pow(1.0f - AngularDrag, deltaTime);

                // Zero out negligible angular velocity to avoid numerical errors
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
            // Skip collision handling for kinematic objects
            if (IsKinematic)
                return;

            // Find all colliders on this GameObject
            List<Collider> attachedColliders = new List<Collider>();
            foreach (var component in GameObject.Components)
            {
                if (component is Collider collider)
                {
                    attachedColliders.Add(collider);
                }
            }

            // If we have no colliders, there's nothing to do
            if (attachedColliders.Count == 0)
                return;

            // Process each collider
            foreach (var collider in attachedColliders)
            {
                // Skip trigger colliders for physics resolution
                if (collider.IsTrigger)
                    continue;

                // Resolve collisions with each colliding object
                foreach (var otherCollider in collidingWith)
                {
                    // Skip trigger colliders for physics resolution
                    if (otherCollider.IsTrigger)
                        continue;

                    // Only resolve if both objects have rigidbodies
                    if (otherCollider.AttachedRigidbody != null)
                    {
                        // Resolve collision between our collider and the other collider
                        collider.ResolveCollision(otherCollider, Time.DeltaTime);
                    }
                }
            }
        }

        private Vector3 TransformDirectionToWorld(Vector3 localDirection)
        {
            if (GameObject?.Transform == null)
                return localDirection;

            // Get rotation from the transform as a quaternion
            Quaternion rotation = Quaternion.FromEulerAngles(
                MathHelper.DegreesToRadians(GameObject.Transform.Rotation.X),
                MathHelper.DegreesToRadians(GameObject.Transform.Rotation.Y),
                MathHelper.DegreesToRadians(GameObject.Transform.Rotation.Z));

            // Multiply quaternion by the vector to rotate it
            Quaternion rotated = rotation * new Quaternion(localDirection.X, localDirection.Y, localDirection.Z, 0) *
                                  Quaternion.Conjugate(rotation);

            // Extract the rotated vector
            return new Vector3(rotated.X, rotated.Y, rotated.Z);
        }

    }
}
