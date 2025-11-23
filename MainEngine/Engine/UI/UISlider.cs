using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using RenbokoEngine.Graphics;
using RenbokoEngine.Input;
using RenbokoEngine.Core;

namespace RenbokoEngine.UI
{
    /// <summary>
    /// Horizontal slider, Value in [0,1].
    /// </summary>
    public class UISlider : UIElement
    {
        public float Value { get; set; } = 0f;
        public Color TrackColor { get; set; } = new Color(60, 60, 70);
        public Color FillColor { get; set; } = new Color(180, 180, 200);
        public int Height { get; set; } = 12;

        private bool _dragging;
        private int _thumbWidth = 12;

        public override void Update()
        {
            if (!Enabled || !Visible) return;

            var input = ServiceLocator.Get<InputSystem>();
            var mouse = input.GetDevice<MouseDevice>();
            if (mouse == null) return;

            var p = mouse.Position;
            var hovered = ContainsPoint(p);

            bool left = mouse.GetButton(MouseButton.Left);
            bool leftDown = mouse.GetButtonDown(MouseButton.Left);

            if (hovered && leftDown) _dragging = true;
            if (!_dragging && leftDown && hovered) _dragging = true;
            if (_dragging && !left) _dragging = false;

            if (_dragging)
            {
                var relativeX = (p.X - Rect.X);
                Value = MathHelper.Clamp(relativeX / (float)Rect.Width, 0f, 1f);
            }
        }

        public override void Draw(Renderer2D renderer, SpriteFont font)
        {
            if (!Visible) return;
            var tex = UIHelpers.GetWhiteTexture(renderer);
            // track
            renderer.Draw(tex, new Microsoft.Xna.Framework.Vector2(Rect.X, Rect.Y + (Rect.Height - Height) / 2f), null, TrackColor, 0f, Microsoft.Xna.Framework.Vector2.Zero, new Microsoft.Xna.Framework.Vector2(Rect.Width, Height));
            // fill
            renderer.Draw(tex, new Microsoft.Xna.Framework.Vector2(Rect.X, Rect.Y + (Rect.Height - Height) / 2f), null, FillColor, 0f, Microsoft.Xna.Framework.Vector2.Zero, new Microsoft.Xna.Framework.Vector2(Rect.Width * Value, Height));
            // thumb
            var thumbX = Rect.X + (Rect.Width * Value) - _thumbWidth / 2f;
            renderer.Draw(tex, new Microsoft.Xna.Framework.Vector2(thumbX, Rect.Y + (Rect.Height - _thumbWidth) / 2f), null, FillColor, 0f, Microsoft.Xna.Framework.Vector2.Zero, new Microsoft.Xna.Framework.Vector2(_thumbWidth, _thumbWidth));
        }
    }
}
