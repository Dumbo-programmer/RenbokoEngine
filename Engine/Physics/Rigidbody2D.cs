using Microsoft.Xna.Framework;

namespace RenyulEngine.Physics
{
    /// <summary>
    /// Represents a 2D physics body with velocity, mass, and forces.
    /// </summary>
    public class Rigidbody2D : Core.IUpdatable
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Mass { get; set; } = 1f;
        public bool IsStatic { get; set; } = false;

        // NEW: restitution (bounciness)
        public float Restitution { get; set; } = 0.5f;

        private Vector2 _forceAccumulator;

        public Collider2D Collider { get; private set; }

        public Rigidbody2D(Collider2D collider)
        {
            Collider = collider;
            collider.AttachRigidbody(this);
        }

        public void AddForce(Vector2 force)
        {
            if (!IsStatic)
                _forceAccumulator += force;
        }

        public void Update()
        {
            if (IsStatic) return;

            // Get time from service locator
            var time = RenyulEngine.Core.ServiceLocator.Get<RenyulEngine.Core.Time>();
            float dt = (float)time.DeltaTime;

            // Acceleration = F / m
            Vector2 acceleration = _forceAccumulator / Mass;
            Velocity += acceleration * dt;
            Position += Velocity * dt;

            Collider?.UpdateBounds(Position);

            // Reset forces for the next frame
            _forceAccumulator = Vector2.Zero;
        }
    }
}
