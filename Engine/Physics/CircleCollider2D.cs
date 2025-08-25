using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System;
namespace RenyulEngine.Physics
{
    /// <summary>
    /// Circle collider for 2D physics.
    /// </summary>
    public class CircleCollider2D : Collider2D
    {
        public float Radius;
        public Microsoft.Xna.Framework.Vector2 Center => Rigidbody?.Position ?? Microsoft.Xna.Framework.Vector2.Zero;

        public CircleCollider2D(float radius)
        {
            Radius = radius;
            Bounds = new AABB(new Microsoft.Xna.Framework.Vector2(-radius), new Microsoft.Xna.Framework.Vector2(radius));
        }

        public override void UpdateBounds(Microsoft.Xna.Framework.Vector2 position)
        {
            Bounds = new AABB(position - new Microsoft.Xna.Framework.Vector2(Radius), position + new Microsoft.Xna.Framework.Vector2(Radius));
        }

        public override bool Intersects(Collider2D other, out Collision collision)
        {
            collision = default;

            if (other is CircleCollider2D circle)
                return CircleCircleIntersection(this, circle, out collision);

            if (other is BoxCollider2D box)
                return box.Intersects(this, out collision); // reuse Box-Circle logic

            return false;
        }

        private bool CircleCircleIntersection(CircleCollider2D a, CircleCollider2D b, out Collision collision)
        {
            collision = default;

            Microsoft.Xna.Framework.Vector2 diff = b.Center - a.Center;
            float distSq = diff.LengthSquared();
            float radiusSum = a.Radius + b.Radius;

            if (distSq > radiusSum * radiusSum)
                return false;

            float dist = MathF.Sqrt(distSq);
            Microsoft.Xna.Framework.Vector2 normal = dist > 0 ? Microsoft.Xna.Framework.Vector2.Normalize(diff) : Microsoft.Xna.Framework.Vector2.UnitY;

            collision = new Collision(a, b, normal, radiusSum - dist);
            return true;
        }
    }
}
