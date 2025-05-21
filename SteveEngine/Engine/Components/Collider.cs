using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace SteveEngine
{
    // Types of colliders
    public enum ColliderType
    {
        Box,
        Sphere,
        Capsule
    }

    public class Collider : Component
    {
        // Basic properties
        public Vector3 Center { get; set; } = Vector3.Zero;  // Local center of the collider
        public bool IsTrigger { get; set; } = false;  // If true, detects collisions without physics response
        public ColliderType Type { get; private set; } = ColliderType.Box;

        // Cached rigidbody reference
        private Rigidbody attachedRigidbody;
        public Rigidbody AttachedRigidbody
        {
            get
            {
                if (attachedRigidbody == null && GameObject != null)
                {
                    // Find the rigidbody on this GameObject
                    foreach (var component in GameObject.Components)
                    {
                        if (component is Rigidbody rb)
                        {
                            attachedRigidbody = rb;
                            break;
                        }
                    }
                }
                return attachedRigidbody;
            }
        }

        // Dimensions for box collider
        public Vector3 Size { get; set; } = Vector3.One;

        // Radius for sphere or capsule
        public float Radius { get; set; } = 0.5f;

        // Height for capsule (total height including hemisphere ends)
        public float Height { get; set; } = 2.0f;

        // Keep track of overlapping colliders
        private List<Collider> overlappingColliders = new List<Collider>();

        // Constructor for different collider types
        public Collider() : this(ColliderType.Box) { }

        public static List<Collider> AllColliders = new();

        public Collider(ColliderType type)
        {
            Type = type;
            AllColliders.Add(this);
        }

        public override void Update(float deltaTime)
        {
            // add stuff
            foreach (var other in AllColliders)
            {
                if (other != this && Intersects(other))
                {
                    ResolveCollision(other, deltaTime);
                }
            }
        }

        // Get the world bounds of this collider
        public (Vector3 min, Vector3 max) GetBounds()
        {
            if (GameObject?.Transform == null)
                return (Vector3.Zero, Vector3.Zero);

            Vector3 worldCenter = GameObject.Transform.Position + Center;

            switch (Type)
            {
                case ColliderType.Box:
                    // Simplified - doesn't account for rotation
                    Vector3 extents = Size * 0.5f;
                    return (worldCenter - extents, worldCenter + extents);

                case ColliderType.Sphere:
                    Vector3 radiusVec = new Vector3(Radius);
                    return (worldCenter - radiusVec, worldCenter + radiusVec);

                case ColliderType.Capsule:
                    // Simplified - assumes Y-axis aligned capsule
                    float halfHeight = Height * 0.5f;
                    Vector3 capsuleExtents = new Vector3(Radius, halfHeight, Radius);
                    return (worldCenter - capsuleExtents, worldCenter + capsuleExtents);

                default:
                    return (worldCenter, worldCenter);
            }
        }

        // Get the world center of this collider
        public Vector3 GetWorldCenter()
        {
            if (GameObject?.Transform == null)
                return Vector3.Zero;

            return GameObject.Transform.Position + Center;
        }

        // Check if this collider intersects with another
        public bool Intersects(Collider other)
        {
            if (GameObject == null || other == null || other.GameObject == null)
                return false;

            // First use AABB test as a quick rejection
            (Vector3 minA, Vector3 maxA) = GetBounds();
            (Vector3 minB, Vector3 maxB) = other.GetBounds();

            bool aabbOverlaps =
                maxA.X >= minB.X && minA.X <= maxB.X &&
                maxA.Y >= minB.Y && minA.Y <= maxB.Y &&
                maxA.Z >= minB.Z && minA.Z <= maxB.Z;

            if (!aabbOverlaps)
                return false;

            // More precise collision tests based on collider types
            bool overlaps = false;

            if (Type == ColliderType.Sphere && other.Type == ColliderType.Sphere)
            {
                overlaps = SphereSphereColl(this, other, out _, out _);
            }
            else if (Type == ColliderType.Box && other.Type == ColliderType.Box)
            {
                overlaps = BoxBoxColl(this, other, out _, out _);
            }
            else if (Type == ColliderType.Sphere && other.Type == ColliderType.Box ||
                    Type == ColliderType.Box && other.Type == ColliderType.Sphere)
            {
                Collider sphere = Type == ColliderType.Sphere ? this : other;
                Collider box = Type == ColliderType.Box ? this : other;
                overlaps = SphereBoxColl(sphere, box, out _, out _);
            }
            else if (Type == ColliderType.Capsule || other.Type == ColliderType.Capsule)
            {
                // Simplified capsule collision using the AABB test result
                overlaps = aabbOverlaps;
            }
            else
            {
                // Fallback to AABB for any other combinations
                overlaps = aabbOverlaps;
            }

            // Update overlapping state if needed
            if (overlaps && !overlappingColliders.Contains(other))
            {
                overlappingColliders.Add(other);
                OnCollisionEnter(other);

                // Update the rigidbody if this is not just a trigger
                if (!IsTrigger && AttachedRigidbody != null)
                {
                    AttachedRigidbody.AddCollision(other);
                }
            }
            else if (!overlaps && overlappingColliders.Contains(other))
            {
                overlappingColliders.Remove(other);
                OnCollisionExit(other);

                // Update the rigidbody
                if (!IsTrigger && AttachedRigidbody != null)
                {
                    AttachedRigidbody.RemoveCollision(other);
                }
            }

            return overlaps;
        }

        // Sphere vs Sphere collision detection
        private static bool SphereSphereColl(Collider sphereA, Collider sphereB,
            out Vector3 normal, out float penetration)
        {
            normal = Vector3.Zero;
            penetration = 0f;

            Vector3 posA = sphereA.GetWorldCenter();
            Vector3 posB = sphereB.GetWorldCenter();

            Vector3 direction = posB - posA;
            float distance = direction.Length;

            // Combined radii
            float radiiSum = sphereA.Radius + sphereB.Radius;

            if (distance >= radiiSum)
                return false;

            // Avoid division by zero
            if (distance > 0.001f)
                normal = direction / distance;
            else
                normal = new Vector3(0, 1, 0); // Arbitrary direction if centers are too close

            penetration = radiiSum - distance;
            return true;
        }

        // Box vs Box collision detection (AABB for simplicity)
        private static bool BoxBoxColl(Collider boxA, Collider boxB,
            out Vector3 normal, out float penetration)
        {
            normal = Vector3.Zero;
            penetration = float.MaxValue;

            Vector3 posA = boxA.GetWorldCenter();
            Vector3 posB = boxB.GetWorldCenter();
            Vector3 extentsA = boxA.Size * 0.5f;
            Vector3 extentsB = boxB.Size * 0.5f;

            // Calculate overlap in each axis
            float overlapX = Math.Min(posA.X + extentsA.X, posB.X + extentsB.X) -
                             Math.Max(posA.X - extentsA.X, posB.X - extentsB.X);
            float overlapY = Math.Min(posA.Y + extentsA.Y, posB.Y + extentsB.Y) -
                             Math.Max(posA.Y - extentsA.Y, posB.Y - extentsB.Y);
            float overlapZ = Math.Min(posA.Z + extentsA.Z, posB.Z + extentsB.Z) -
                             Math.Max(posA.Z - extentsA.Z, posB.Z - extentsB.Z);

            if (overlapX <= 0 || overlapY <= 0 || overlapZ <= 0)
                return false;

            // Use the minimum overlap axis as the collision normal
            if (overlapX < overlapY && overlapX < overlapZ)
            {
                normal = posA.X < posB.X ? new Vector3(-1, 0, 0) : new Vector3(1, 0, 0);
                penetration = overlapX;
            }
            else if (overlapY < overlapZ)
            {
                normal = posA.Y < posB.Y ? new Vector3(0, -1, 0) : new Vector3(0, 1, 0);
                penetration = overlapY;
            }
            else
            {
                normal = posA.Z < posB.Z ? new Vector3(0, 0, -1) : new Vector3(0, 0, 1);
                penetration = overlapZ;
            }

            return true;
        }

        // Sphere vs Box collision detection
        private static bool SphereBoxColl(Collider sphere, Collider box,
            out Vector3 normal, out float penetration)
        {
            normal = Vector3.Zero;
            penetration = 0f;

            Vector3 sphereCenter = sphere.GetWorldCenter();
            Vector3 boxCenter = box.GetWorldCenter();
            Vector3 boxHalfSize = box.Size * 0.5f;

            // Find closest point on box to sphere center
            Vector3 closestPoint = Vector3.Zero;

            // For each axis, clamp sphere center to box bounds
            closestPoint.X = Math.Clamp(sphereCenter.X, boxCenter.X - boxHalfSize.X, boxCenter.X + boxHalfSize.X);
            closestPoint.Y = Math.Clamp(sphereCenter.Y, boxCenter.Y - boxHalfSize.Y, boxCenter.Y + boxHalfSize.Y);
            closestPoint.Z = Math.Clamp(sphereCenter.Z, boxCenter.Z - boxHalfSize.Z, boxCenter.Z + boxHalfSize.Z);

            // Check if the closest point is inside the sphere
            Vector3 delta = sphereCenter - closestPoint;
            float distanceSquared = delta.LengthSquared;
            float radiusSquared = sphere.Radius * sphere.Radius;

            if (distanceSquared >= radiusSquared)
                return false;

            float distance = (float)Math.Sqrt(distanceSquared);

            // Avoid division by zero
            if (distance > 0.001f)
                normal = delta / distance;
            else
                normal = new Vector3(0, 1, 0); // Arbitrary direction

            penetration = sphere.Radius - distance;
            return true;
        }

        public void ResolveCollision(Collider other, float deltaTime)
        {
            Rigidbody rbA = this.AttachedRigidbody;
            Rigidbody rbB = other.AttachedRigidbody;

            // Don't automatically zero the other object's velocity
            // rbB.Velocity = Vector3.Zero; // Removed this line

            Vector3 normal = GetCollisionNormalWith(other); // Direction from A to B
            float restitution = 0f; // Bounciness, 0 = no bounce, 1 = full bounce

            // Relative velocity
            Vector3 relativeVelocity = rbB.Velocity - rbA.Velocity;
            float velAlongNormal = Vector3.Dot(relativeVelocity, normal);

            // Only resolve if they are moving toward each other
            if (velAlongNormal > 0)
                return;

            // Calculate impulse scalar
            float invMassA = 1.0f / rbA.Mass;
            float invMassB = 1.0f / rbB.Mass;

            // Check if either object has infinite mass
            bool isAStationary = rbA.Mass >= float.MaxValue;
            bool isBStationary = rbB.Mass >= float.MaxValue;

            if (isAStationary && isBStationary)
                return; // Both objects are immovable

            // Handle infinite mass objects by zeroing their inverse mass
            if (isAStationary) invMassA = 0f;
            if (isBStationary) invMassB = 0f;

            float j = -(1 + restitution) * velAlongNormal;
            j /= invMassA + invMassB;

            Vector3 impulse = j * normal;

            // Apply impulse
            rbA.Velocity -= impulse * invMassA;
            rbB.Velocity += impulse * invMassB;

            // Positional correction to avoid sinking
            float penetrationDepth = GetPenetrationDepthWith(other);

            // Only perform correction if there's actual penetration
            if (penetrationDepth > 0)
            {
                const float percent = 0.2f; // Penetration correction percentage (0.2 to 0.8)
                const float slop = 0.01f;   // Small threshold to ignore tiny penetrations

                // Only correct if penetration exceeds the slop
                if (penetrationDepth > slop)
                {
                    Vector3 correction = normal * (penetrationDepth - slop) * percent / (invMassA + invMassB);

                    // Apply correction proportional to inverse mass (static objects don't move)
                    rbA.GameObject.Transform.Position -= correction * invMassA;
                    rbB.GameObject.Transform.Position += correction * invMassB;
                }
            }
        }


        // Event methods
        protected virtual void OnCollisionEnter(Collider other)
        {
            // This could trigger an event for gameplay code to react to
            Console.WriteLine($"Collision between {GameObject.Name} and {other.GameObject.Name}");
        }

        protected virtual void OnCollisionExit(Collider other)
        {
            Console.WriteLine($"Collision exit between {GameObject.Name} and {other.GameObject.Name}");
        }

        // Add the missing method GetCollisionNormalWith to the Collider class.  
        private Vector3 GetCollisionNormalWith(Collider other)
        {
            Vector3 normal = Vector3.Zero;

            // Simplified logic to calculate collision normal based on collider types.  
            if (Type == ColliderType.Sphere && other.Type == ColliderType.Sphere)
            {
                Vector3 direction = other.GetWorldCenter() - GetWorldCenter();
                normal = direction.Normalized();
            }
            else if (Type == ColliderType.Box && other.Type == ColliderType.Box)
            {
                // Use the BoxBoxColl method to calculate the normal.  
                BoxBoxColl(this, other, out normal, out _);
            }
            else if ((Type == ColliderType.Sphere && other.Type == ColliderType.Box) ||
                     (Type == ColliderType.Box && other.Type == ColliderType.Sphere))
            {
                // Use the SphereBoxColl method to calculate the normal.  
                SphereBoxColl(this, other, out normal, out _);
            }
            else
            {
                // Default to a zero vector if no specific logic is implemented.  
                normal = Vector3.Zero;
            }

            return normal;
        }

        private float GetPenetrationDepthWith(Collider other)
        {
            float penetrationDepth = 0f;

            // Simplified logic to calculate penetration depth based on collider types.
            if (Type == ColliderType.Sphere && other.Type == ColliderType.Sphere)
            {
                Vector3 direction = other.GetWorldCenter() - GetWorldCenter();
                float distance = direction.Length;
                float radiiSum = Radius + other.Radius;

                if (distance < radiiSum)
                {
                    penetrationDepth = radiiSum - distance;
                }
            }
            else if (Type == ColliderType.Box && other.Type == ColliderType.Box)
            {
                // Use the BoxBoxColl method to calculate the penetration depth.
                BoxBoxColl(this, other, out _, out penetrationDepth);
            }
            else if ((Type == ColliderType.Sphere && other.Type == ColliderType.Box) ||
                     (Type == ColliderType.Box && other.Type == ColliderType.Sphere))
            {
                // Use the SphereBoxColl method to calculate the penetration depth.
                SphereBoxColl(this, other, out _, out penetrationDepth);
            }
            else
            {
                // Default to zero if no specific logic is implemented.
                penetrationDepth = 0f;
            }

            return penetrationDepth;
        }
    }
}
