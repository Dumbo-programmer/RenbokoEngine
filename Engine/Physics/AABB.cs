using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace RenyulEngine.Physics
{
    /// <summary>
    /// Axis-Aligned Bounding Box for collision detection.
    /// </summary>
    public struct AABB
    {
        public Microsoft.Xna.Framework.Vector2 Min;
        public Microsoft.Xna.Framework.Vector2 Max;

        public AABB(Microsoft.Xna.Framework.Vector2 min, Microsoft.Xna.Framework.Vector2 max)
        {
            Min = min;
            Max = max;
        }

        public bool Intersects(AABB other)
        {
            return !(Max.X < other.Min.X ||
                     Min.X > other.Max.X ||
                     Max.Y < other.Min.Y ||
                     Min.Y > other.Max.Y);
        }

        public Microsoft.Xna.Framework.Vector2 GetCenter() => (Min + Max) * 0.5f;
        public Microsoft.Xna.Framework.Vector2 GetSize() => Max - Min;
    }
}
