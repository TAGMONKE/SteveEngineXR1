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

        public Collider(ColliderType type)
        {
            Type = type;
        }

        public override void Update(float deltaTime)
        {
            // Physics engines typically handle collision detection in a separate system
            // rather than in the Update method of individual colliders
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
            else if ((Type == ColliderType.Sphere && other.Type == ColliderType.Box) ||
                    (Type == ColliderType.Box && other.Type == ColliderType.Sphere))
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

        // Calculate more accurate collision response
        public void ResolveCollision(Collider other, float deltaTime)
        {
            if (IsTrigger || other.IsTrigger)
                return; // Triggers don't have physical responses

            if (AttachedRigidbody == null || other.AttachedRigidbody == null)
                return; // Need rigidbodies to resolve collisions

            // Get collision details based on collider types
            Vector3 normal;
            float penetration;
            bool collided = false;

            if (Type == ColliderType.Sphere && other.Type == ColliderType.Sphere)
            {
                collided = SphereSphereColl(this, other, out normal, out penetration);
            }
            else if (Type == ColliderType.Box && other.Type == ColliderType.Box)
            {
                collided = BoxBoxColl(this, other, out normal, out penetration);
            }
            else if (Type == ColliderType.Sphere && other.Type == ColliderType.Box)
            {
                collided = SphereBoxColl(this, other, out normal, out penetration);
                // Reverse normal since sphere is the first parameter
                normal = -normal;
            }
            else if (Type == ColliderType.Box && other.Type == ColliderType.Sphere)
            {
                collided = SphereBoxColl(other, this, out normal, out penetration);
            }
            else
            {
                // Simplified fallback for other combinations
                Vector3 posA = GameObject.Transform.Position + Center;
                Vector3 posB = other.GameObject.Transform.Position + other.Center;
                normal = Vector3.Normalize(posB - posA);
                penetration = 0.1f; // Default penetration
                collided = true;
            }

            if (!collided)
                return;

            // Position correction to resolve penetration
            float totalMass = AttachedRigidbody.Mass + other.AttachedRigidbody.Mass;
            float ratioA = AttachedRigidbody.IsKinematic ? 0 : other.AttachedRigidbody.Mass / totalMass;
            float ratioB = other.AttachedRigidbody.IsKinematic ? 0 : AttachedRigidbody.Mass / totalMass;

            // Adjust penetration resolution strength
            float resolutionFactor = 0.8f;

            // Apply position correction if not kinematic
            if (!AttachedRigidbody.IsKinematic)
                GameObject.Transform.Position -= normal * penetration * ratioA * resolutionFactor;

            if (!other.AttachedRigidbody.IsKinematic)
                other.GameObject.Transform.Position += normal * penetration * ratioB * resolutionFactor;

            // Velocity response (improved elastic collision)
            Vector3 relativeVelocity = other.AttachedRigidbody.Velocity - AttachedRigidbody.Velocity;
            float velocityAlongNormal = Vector3.Dot(relativeVelocity, normal);

            // Only respond if objects are moving toward each other
            if (velocityAlongNormal > 0)
                return;

            // Restitution (bounciness) - a value between 0 and 1
            float restitution = 0.6f;

            // Calculate impulse scalar with improved formula
            float inverseMassA = AttachedRigidbody.IsKinematic ? 0 : 1 / AttachedRigidbody.Mass;
            float inverseMassB = other.AttachedRigidbody.IsKinematic ? 0 : 1 / other.AttachedRigidbody.Mass;

            // Avoid division by zero
            if (inverseMassA == 0 && inverseMassB == 0)
                return;

            float j = -(1 + restitution) * velocityAlongNormal;
            j /= inverseMassA + inverseMassB;

            // Apply impulse
            Vector3 impulse = j * normal;
            if (!AttachedRigidbody.IsKinematic)
                AttachedRigidbody.AddForce(-impulse, ForceMode.Impulse);

            if (!other.AttachedRigidbody.IsKinematic)
                other.AttachedRigidbody.AddForce(impulse, ForceMode.Impulse);

            // Apply friction
            const float frictionCoefficient = 0.3f;

            // Calculate tangent vector
            Vector3 tangent = relativeVelocity - velocityAlongNormal * normal;
            float tangentLength = tangent.Length;

            // Only apply friction if there's a tangential component
            if (tangentLength > 0.001f)
            {
                tangent = Vector3.Normalize(tangent);

                // Calculate friction impulse
                float frictionImpulse = -Vector3.Dot(relativeVelocity, tangent);
                frictionImpulse /= inverseMassA + inverseMassB;
                frictionImpulse *= frictionCoefficient;

                // Apply friction impulse
                Vector3 frictionForce = frictionImpulse * tangent;
                if (!AttachedRigidbody.IsKinematic)
                    AttachedRigidbody.AddForce(-frictionForce, ForceMode.Impulse);

                if (!other.AttachedRigidbody.IsKinematic)
                    other.AttachedRigidbody.AddForce(frictionForce, ForceMode.Impulse);
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
    }
}
