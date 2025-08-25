using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using RenyulEngine.Graphics;

namespace RenyulEngine.UI
{
    public class UILabel : UIElement
    {
        public string Text { get; set; } = "";
        public Color Color { get; set; } = Color.White;
        public float Scale { get; set; } = 1f;
        public bool Wrap { get; set; } = false;
        public enum Alignment { Left, Center, Right }
        public Alignment Align { get; set; } = Alignment.Left;

        public override void Draw(Renderer2D renderer, SpriteFont font)
        {
            if (!Visible || font == null) return;

            var pos = new Microsoft.Xna.Framework.Vector2(Rect.X, Rect.Y);
            Microsoft.Xna.Framework.Vector2 origin = Microsoft.Xna.Framework.Vector2.Zero;
            if (Align != Alignment.Left)
            {
                var size = font.MeasureString(Text) * Scale;
                if (Align == Alignment.Center) origin = new Microsoft.Xna.Framework.Vector2(size.X / 2f, 0);
                else origin = new Microsoft.Xna.Framework.Vector2(size.X, 0);

                pos = new Microsoft.Xna.Framework.Vector2(Rect.X, Rect.Y);
            }

            renderer.DrawString(font, Text, pos, Color);
        }
    }
}
