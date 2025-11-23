using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenbokoEngine.Graphics;
using RenbokoEngine.Input;
using RenbokoEngine.Core;

namespace RenbokoEngine.UI
{
    public class UIButton : UIElement
    {
        public string Text { get; set; } = "Button";
        public Color Background { get; set; } = new Color(40, 45, 50);
        public Color HoverBackground { get; set; } = new Color(60, 70, 80);
        public Color TextColor { get; set; } = Color.White;
        public int CornerRadius { get; set; } = 4;

        public System.Action? OnClick;

        private bool _hover;
        private bool _pressed;

        public override void Update()
        {
            if (!Enabled || !Visible) return;

            var input = ServiceLocator.Get<InputSystem>();
            var mouse = input.GetDevice<MouseDevice>();
            if (mouse == null) return;

            var p = mouse.Position;
            _hover = ContainsPoint(p);

            bool leftDown = mouse.GetButton(MouseButton.Left);
            bool leftDownThisFrame = mouse.GetButtonDown(MouseButton.Left);

            if (_hover && leftDownThisFrame) _pressed = true;

            // click occurs on release inside the button
            if (_pressed && !leftDown)
            {
                if (_hover) OnClick?.Invoke();
                _pressed = false;
            }
        }

        public override void Draw(Renderer2D renderer, SpriteFont font)
        {
            if (!Visible) return;

            var tex = UIHelpers.GetWhiteTexture(renderer);
            var bg = _hover ? HoverBackground : Background;

            // draw rectangle
            renderer.Draw(tex, new Vector2(GetAbsoluteRect().X, GetAbsoluteRect().Y), null, bg, 0f, Vector2.Zero, new Vector2(Rect.Width, Rect.Height));

            // draw text centered
            if (font != null)
            {
                var textSize = font.MeasureString(Text);
                var textPos = new Vector2(GetAbsoluteRect().X + (Rect.Width - textSize.X) / 2f,
                                          GetAbsoluteRect().Y + (Rect.Height - textSize.Y) / 2f);
                renderer.DrawString(font, Text, textPos, TextColor);
            }
        }
    }
}
