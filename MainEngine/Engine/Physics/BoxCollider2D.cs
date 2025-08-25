using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System;
namespace RenyulEngine.Physics
{
    /// <summary>
    /// Axis-aligned box collider.
    /// </summary>
    public class BoxCollider2D : Collider2D
    {
        public Microsoft.Xna.Framework.Vector2 Size;

        public BoxCollider2D(Microsoft.Xna.Framework.Vector2 size)
        {
            Size = size;
            Bounds = new AABB(Microsoft.Xna.Framework.Vector2.Zero, size);
        }

        public override void UpdateBounds(Microsoft.Xna.Framework.Vector2 position)
        {
            Microsoft.Xna.Framework.Vector2 halfSize = Size * 0.5f;
            Bounds = new AABB(position - halfSize, position + halfSize);
        }

        public override bool Intersects(Collider2D other, out Collision collision)
        {
            collision = default;

            if (other is BoxCollider2D box)
                return BoxBoxIntersection(this, box, out collision);

            if (other is CircleCollider2D circle)
                return BoxCircleIntersection(this, circle, out collision);

            return false;
        }

        private bool BoxBoxIntersection(BoxCollider2D a, BoxCollider2D b, out Collision collision)
        {
            collision = default;
            if (!a.Bounds.Intersects(b.Bounds))
                return false;

            // Compute overlap
            Microsoft.Xna.Framework.Vector2 aCenter = a.Bounds.GetCenter();
            Microsoft.Xna.Framework.Vector2 bCenter = b.Bounds.GetCenter();
            Microsoft.Xna.Framework.Vector2 distance = bCenter - aCenter;
            Microsoft.Xna.Framework.Vector2 overlap = (a.Size * 0.5f + b.Size * 0.5f) - new Microsoft.Xna.Framework.Vector2(MathF.Abs(distance.X), MathF.Abs(distance.Y));

            if (overlap.X < overlap.Y)
            {
                float normalX = distance.X < 0 ? -1 : 1;
                collision = new Collision(a, b, new Microsoft.Xna.Framework.Vector2(normalX, 0), overlap.X);
            }
            else
            {
                float normalY = distance.Y < 0 ? -1 : 1;
                collision = new Collision(a, b, new Microsoft.Xna.Framework.Vector2(0, normalY), overlap.Y);
            }

            return true;
        }

        private bool BoxCircleIntersection(BoxCollider2D box, CircleCollider2D circle, out Collision collision)
        {
            collision = default;

            Microsoft.Xna.Framework.Vector2 boxCenter = box.Bounds.GetCenter();
            Microsoft.Xna.Framework.Vector2 halfSize = box.Size * 0.5f;

            // Clamp circle center to box edges
            Microsoft.Xna.Framework.Vector2 closestPoint = Microsoft.Xna.Framework.Vector2.Clamp(circle.Center, boxCenter - halfSize, boxCenter + halfSize);

            Microsoft.Xna.Framework.Vector2 diff = circle.Center - closestPoint;
            float distSq = diff.LengthSquared();

            if (distSq > circle.Radius * circle.Radius)
                return false;

            float dist = MathF.Sqrt(distSq);
            Microsoft.Xna.Framework.Vector2 normal = dist > 0 ? Microsoft.Xna.Framework.Vector2.Normalize(diff) : Microsoft.Xna.Framework.Vector2.UnitY;

            collision = new Collision(box, circle, normal, circle.Radius - dist);
            return true;
        }
    }
}
