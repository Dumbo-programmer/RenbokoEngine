using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RenbokoEngine.Physics
{
    /// <summary>
    /// Handles physics simulation, updates rigidbodies, and resolves collisions.
    /// </summary>
    public class PhysicsWorld : Core.IService
    {
        private readonly List<Rigidbody2D> _bodies = new();
        private readonly List<Collider2D> _colliders = new();

        public void AddBody(Rigidbody2D body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (!_bodies.Contains(body))
                _bodies.Add(body);
            if (body.Collider != null && !_colliders.Contains(body.Collider))
                _colliders.Add(body.Collider);
        }

        /// <summary>
        /// Step the physics world with an explicit fixed delta time.
        /// Call this from your game loop (EngineGame) with the fixed dt.
        /// </summary>
        public void Step(double dt)
        {
            // If your Rigidbody2D.Update reads Time from ServiceLocator, set it here:
            var time = Core.ServiceLocator.Has<Core.Time>() ? Core.ServiceLocator.Get<Core.Time>() : null;
            if (time != null) time.FixedDelta = dt;

            Update();
        }
        public static float Clamp(float value, float min, float max)
    => (value < min) ? min : (value > max) ? max : value;

        /// <summary>
        /// Internal update (uses Time.FixedDelta if rigidbodies rely on Time service).
        /// </summary>
        public void Update()
        {
            // Update all bodies
            foreach (var body in _bodies)
                body.Update();

            // Detect and resolve collisions
            for (int i = 0; i < _colliders.Count; i++)
            {
                for (int j = i + 1; j < _colliders.Count; j++)
                {
                    var a = _colliders[i];
                    var b = _colliders[j];

                    if (a == null || b == null) continue;

                    if (a.Intersects(b, out var collision) && collision != null)
                        ResolveCollision(collision);
                }
            }
        }

        private void ResolveCollision(Collision collision)
        {
            if (collision == null) return;

            var aRb = collision.A?.Rigidbody;
            var bRb = collision.B?.Rigidbody;

            // If either collider has no body, skip (or implement static world collision handling)
            if (aRb == null || bRb == null) return;

            if (aRb.IsStatic && bRb.IsStatic) return;

            // position correction (split between bodies)
            Vector2 correction = collision.Normal * collision.Penetration * 0.5f;

            if (!aRb.IsStatic)
                aRb.Position -= correction;
            if (!bRb.IsStatic)
                bRb.Position += correction;

            // relative velocity
            Vector2 relativeVelocity = bRb.Velocity - aRb.Velocity;
            float velAlongNormal = Vector2.Dot(relativeVelocity, collision.Normal);

            // if velocities are separating, do nothing
            if (velAlongNormal > 0f) return;

            // basic impulse resolution with restitution (bounciness)
            float restitution = Clamp(collision.Restitution, 0f, 1f);
            if (float.IsNaN(restitution)) restitution = 0.5f;

            // avoid divide by zero (mass==0 means infinite mass => immovable)
            float invMassA = aRb.Mass > 0f ? 1f / aRb.Mass : 0f;
            float invMassB = bRb.Mass > 0f ? 1f / bRb.Mass : 0f;
            float denom = invMassA + invMassB;
            if (denom <= 0f) return;

            float j = -(1f + restitution) * velAlongNormal;
            j /= denom;

            Vector2 impulse = j * collision.Normal;

            if (!aRb.IsStatic && aRb.Mass > 0f)
                aRb.Velocity -= impulse * invMassA;
            if (!bRb.IsStatic && bRb.Mass > 0f)
                bRb.Velocity += impulse * invMassB;
        }
    }
}
