using Microsoft.Xna.Framework;

namespace RenyulEngine.Physics
{
    /// <summary>
    /// Base class for 2D colliders.
    /// </summary>
    public abstract class Collider2D
    {
        public Rigidbody2D Rigidbody { get; private set; }
        public AABB Bounds { get; protected set; }

        // Restitution reads from Rigidbody if attached, else defaults to 0.5
        public float Restitution => Rigidbody?.Restitution ?? 0.5f;

        public void AttachRigidbody(Rigidbody2D rb)
        {
            Rigidbody = rb;
        }

        public abstract void UpdateBounds(Vector2 position);
        public abstract bool Intersects(Collider2D other, out Collision collision);
    }
}
