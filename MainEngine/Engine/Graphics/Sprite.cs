using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RenbokoEngine.Graphics
{
    public class Sprite
    {
        public Texture2D Texture { get; }
        public Rectangle? Source;
        public Vector2 Origin;
        public Color Color = Color.White;
        public Vector2 Scale = Vector2.One;
        public float Rotation = 0f;
        public float Layer = 0f;

        public Sprite(Texture2D tex)
        {
            Texture = tex;
        }

        public void Draw(Renderer2D renderer, Vector2 position)
            => renderer.Draw(Texture, position, Source, Color, Rotation, Origin, Scale, SpriteEffects.None, Layer);
    }
}
