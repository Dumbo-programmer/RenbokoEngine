using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using Microsoft.Xna.Framework.Input;
using RenbokoEngine.Graphics;
using RenbokoEngine.Input;
using RenbokoEngine.Core;

namespace RenbokoEngine.UI
{
    /// <summary>
    /// Basic single-line text input. Click to focus, type letters/numbers, backspace supported.
    /// (Not IME-aware — good for ASCII input).
    /// </summary>
    public class UIInputField : UIElement
    {
        public string Text { get; set; } = "";
        public Color Background { get; set; } = new Color(30, 30, 35);
        public Color TextColor { get; set; } = Color.White;
        public bool Focused { get; private set; } = false;
        public int MaxLength { get; set; } = 256;
        public System.Action<string>? OnSubmit; // executed when Enter is pressed

        private float _caretTimer = 0f;
        private bool _caretVisible = true;

        public override void Update()
        {
            if (!Enabled || !Visible) return;

            var input = ServiceLocator.Get<InputSystem>();
            var mouse = input.GetDevice<MouseDevice>();
            var keyboard = input.GetDevice<KeyboardDevice>();

            if (mouse != null && mouse.GetButtonDown(MouseButton.Left))
            {
                Focused = ContainsPoint(mouse.Position);
            }

            if (!Focused) return;

            // caret blink
            _caretTimer += (float)Core.ServiceLocator.Get<Time>().DeltaTime;
            if (_caretTimer > 0.5f) { _caretTimer = 0f; _caretVisible = !_caretVisible; }

            if (keyboard != null)
            {
                // backspace
                if (keyboard.GetKeyDown(Keys.Back) && Text.Length > 0)
                {
                    Text = Text.Substring(0, Text.Length - 1);
                }

                // Enter -> submit
                if (keyboard.GetKeyDown(Keys.Enter))
                {
                    OnSubmit?.Invoke(Text);
                    Focused = false;
                }

                // letters A-Z
                for (Keys k = Keys.A; k <= Keys.Z; k++)
                {
                    if (keyboard.GetKeyDown(k) && Text.Length < MaxLength)
                    {
                        bool shift = keyboard.GetKey(Keys.LeftShift) || keyboard.GetKey(Keys.RightShift);
                        char c = (char)((int)'a' + (k - Keys.A));
                        if (shift) c = char.ToUpper(c);
                        Text += c;
                    }
                }

                // digits
                for (Keys k = Keys.D0; k <= Keys.D9; k++)
                {
                    if (keyboard.GetKeyDown(k) && Text.Length < MaxLength)
                    {
                        char c = (char)('0' + (k - Keys.D0));
                        Text += c;
                    }
                }

                // space
                if (keyboard.GetKeyDown(Keys.Space) && Text.Length < MaxLength)
                    Text += ' ';

                // simple punctuation
                if (keyboard.GetKeyDown(Keys.OemPeriod) && Text.Length < MaxLength) Text += '.';
                if (keyboard.GetKeyDown(Keys.OemComma) && Text.Length < MaxLength) Text += ',';
                if (keyboard.GetKeyDown(Keys.OemMinus) && Text.Length < MaxLength) Text += '-';
                if (keyboard.GetKeyDown(Keys.OemPlus) && Text.Length < MaxLength) Text += '+';
            }
        }

        public override void Draw(Renderer2D renderer, SpriteFont font)
        {
            if (!Visible) return;
            var tex = UIHelpers.GetWhiteTexture(renderer);

            renderer.Draw(tex, new Microsoft.Xna.Framework.Vector2(Rect.X, Rect.Y), null, Background, 0f, Microsoft.Xna.Framework.Vector2.Zero, new Microsoft.Xna.Framework.Vector2(Rect.Width, Rect.Height));

            if (font != null)
            {
                var pos = new Microsoft.Xna.Framework.Vector2(Rect.X + 6, Rect.Y + (Rect.Height - font.LineSpacing) / 2f);
                renderer.DrawString(font, Text, pos, TextColor);

                // caret
                if (Focused && _caretVisible)
                {
                    var measure = font.MeasureString(Text);
                    var caretX = pos.X + measure.X + 2;
                    renderer.Draw(tex, new Microsoft.Xna.Framework.Vector2(caretX, Rect.Y + 4), null, TextColor, 0f, Microsoft.Xna.Framework.Vector2.Zero, new Microsoft.Xna.Framework.Vector2(2, Rect.Height - 8));
                }
            }
        }
    }
}
