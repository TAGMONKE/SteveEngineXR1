using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace SteveEngine
{
    public static class Time
    {
        public static float DeltaTime { get; set; }
        public static float FixedDeltaTime { get; set; } = 0.02f; // Fixed physics timestep (50 Hz)
        public static float TotalTime { get; set; }
        
        // For accumulating time between fixed updates
        private static float accumulatedTime;
        
        // Update the time from the main game loop
        public static void Update(float deltaTime)
        {
            DeltaTime = deltaTime;
            TotalTime += deltaTime;
            accumulatedTime += deltaTime;
        }
        
        // Check if enough time has accumulated for a physics update
        public static bool ShouldUpdatePhysics()
        {
            if (accumulatedTime >= FixedDeltaTime)
            {
                accumulatedTime -= FixedDeltaTime;
                return true;
            }
            return false;
        }
    }
    
    public static class Physics
    {
        // List of all colliders in the scene
        private static List<Collider> colliders = new List<Collider>();
        
        // List of all rigidbodies in the scene
        private static List<Rigidbody> rigidbodies = new List<Rigidbody>();
        
        // Settings
        public static Vector3 Gravity { get; set; } = new Vector3(0, -9.81f, 0);
        public static int SolverIterations { get; set; } = 6;
        public static bool UseCollisionDetection { get; set; } = true;
        
        // Register a new collider in the physics system
        public static void RegisterCollider(Collider collider)
        {
            if (!colliders.Contains(collider))
            {
                colliders.Add(collider);
            }
        }
        
        // Unregister a collider
        public static void UnregisterCollider(Collider collider)
        {
            colliders.Remove(collider);
        }
        
        // Register a new rigidbody
        public static void RegisterRigidbody(Rigidbody rigidbody)
        {
            if (!rigidbodies.Contains(rigidbody))
            {
                rigidbodies.Add(rigidbody);
            }
        }
        
        // Unregister a rigidbody
        public static void UnregisterRigidbody(Rigidbody rigidbody)
        {
            rigidbodies.Remove(rigidbody);
        }
        
        // Update the physics simulation
        public static void FixedUpdate()
        {
            // Update all rigidbodies first
            foreach (var rb in rigidbodies)
            {
                if (rb.GameObject != null)
                {
                    rb.Update(Time.FixedDeltaTime);
                }
            }
            
            // Then detect and resolve collisions
            if (UseCollisionDetection)
            {
                DetectAndResolveCollisions();
            }
        }
        
        // Basic collision detection and resolution
        private static void DetectAndResolveCollisions()
        {
            // This is an O(n²) approach, a real physics engine would use spatial partitioning
            for (int i = 0; i < colliders.Count; i++)
            {
                for (int j = i + 1; j < colliders.Count; j++)
                {
                    var colliderA = colliders[i];
                    var colliderB = colliders[j];
                    
                    // Skip if either collider has no GameObject
                    if (colliderA.GameObject == null || colliderB.GameObject == null)
                        continue;
                    
                    // Check for collision
                    if (colliderA.Intersects(colliderB))
                    {
                        // Resolve the collision if neither is a trigger
                        if (!colliderA.IsTrigger && !colliderB.IsTrigger)
                        {
                            colliderA.ResolveCollision(colliderB, Time.FixedDeltaTime);
                        }
                    }
                }
            }
        }
        
        // Raycast into the scene
        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance = float.MaxValue)
        {
            hit = new RaycastHit();
            
            // Normalize the direction
            direction = Vector3.Normalize(direction);
            
            // Track the closest hit
            float closestHitDistance = maxDistance;
            Collider closestCollider = null;
            Vector3 closestPoint = Vector3.Zero;
            
            foreach (var collider in colliders)
            {
                if (collider.GameObject == null)
                    continue;
                
                // This is a simplified raycast test that only works for AABBs
                (Vector3 min, Vector3 max) = collider.GetBounds();
                
                // Ray-AABB intersection test
                float tmin = (min.X - origin.X) / direction.X;
                float tmax = (max.X - origin.X) / direction.X;
                
                if (tmin > tmax)
                {
                    float temp = tmin;
                    tmin = tmax;
                    tmax = temp;
                }
                
                float tymin = (min.Y - origin.Y) / direction.Y;
                float tymax = (max.Y - origin.Y) / direction.Y;
                
                if (tymin > tymax)
                {
                    float temp = tymin;
                    tymin = tymax;
                    tymax = temp;
                }
                
                if ((tmin > tymax) || (tymin > tmax))
                    continue;
                
                if (tymin > tmin)
                    tmin = tymin;
                
                if (tymax < tmax)
                    tmax = tymax;
                
                float tzmin = (min.Z - origin.Z) / direction.Z;
                float tzmax = (max.Z - origin.Z) / direction.Z;
                
                if (tzmin > tzmax)
                {
                    float temp = tzmin;
                    tzmin = tzmax;
                    tzmax = temp;
                }
                
                if ((tmin > tzmax) || (tzmin > tmax))
                    continue;
                
                if (tzmin > tmin)
                    tmin = tzmin;
                
                if (tzmax < tmax)
                    tmax = tzmax;
                
                // We have a hit if tmin is positive (hit is in front of origin)
                // and less than our current closest hit
                if (tmin > 0 && tmin < closestHitDistance)
                {
                    closestHitDistance = tmin;
                    closestCollider = collider;
                    closestPoint = origin + direction * tmin;
                }
            }
            
            // If we found a collision, fill the hit data
            if (closestCollider != null)
            {
                hit.Point = closestPoint;
                hit.Distance = closestHitDistance;
                hit.Collider = closestCollider;
                hit.GameObject = closestCollider.GameObject;
                
                // Calculate normal (very simplified)
                hit.Normal = Vector3.Normalize(closestPoint - (closestCollider.GameObject.Transform.Position + closestCollider.Center));
                
                return true;
            }
            
            return false;
        }
        
        // Check if a collider overlaps with any other colliders
        public static bool OverlapCollider(Collider collider, List<Collider> results)
        {
            bool foundAny = false;
            
            foreach (var other in colliders)
            {
                if (other == collider || other.GameObject == null)
                    continue;
                
                if (collider.Intersects(other))
                {
                    results.Add(other);
                    foundAny = true;
                }
            }
            
            return foundAny;
        }
    }
    
    // Structure to hold raycast hit information
    public struct RaycastHit
    {
        public Vector3 Point;     // Hit point in world space
        public Vector3 Normal;    // Surface normal at hit point
        public float Distance;    // Distance from ray origin to hit point
        public Collider Collider; // Collider that was hit
        public GameObject GameObject; // GameObject that was hit
    }
}
