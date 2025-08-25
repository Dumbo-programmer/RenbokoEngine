using Microsoft.Xna.Framework;

namespace RenyulEngine.Physics
{
    /// <summary>
    /// Collision information for physics resolution.
    /// </summary>
    public class Collision
    {
        public Collider2D A { get; set; }
        public Collider2D B { get; set; }
        public Vector2 Normal { get; set; }
        public float Penetration { get; set; }
        public float Restitution { get; set; } = 0.5f;
        public Rigidbody2D Rigidbody { get; set; }

        public Collision(Collider2D a, Collider2D b, Vector2 normal, float penetration)
        {
            A = a;
            B = b;
            Normal = normal;
            Penetration = penetration;
        }
    }
}
