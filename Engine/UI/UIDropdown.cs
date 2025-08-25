using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using RenyulEngine.Graphics;
using RenyulEngine.Input;
using RenyulEngine.Core;

namespace RenyulEngine.UI
{
    /// <summary>
    /// Simple dropdown. Click to open list; clicking an item selects it.
    /// </summary>
    public class UIDropdown : UIElement
    {
        public List<string> Options { get; set; } = new();
        public int SelectedIndex { get; set; } = -1;
        public bool IsOpen { get; private set; } = false;
        public Color Background { get; set; } = new Color(40, 45, 50);
        public Color HoverBackground { get; set; } = new Color(60, 70, 80);
        public Color TextColor { get; set; } = Color.White;
        public int ItemHeight { get; set; } = 28;

        public System.Action<int, string>? OnSelectionChanged;

        public override void Update()
        {
            if (!Enabled || !Visible) return;

            var input = ServiceLocator.Get<InputSystem>();
            var mouse = input.GetDevice<MouseDevice>();
            if (mouse == null) return;

            var p = mouse.Position;
            bool leftDown = mouse.GetButtonDown(MouseButton.Left);
            bool left = mouse.GetButton(MouseButton.Left);

            if (!IsOpen)
            {
                if (ContainsPoint(p) && leftDown) IsOpen = true;
            }
            else
            {
                // check if click on any item
                for (int i = 0; i < Options.Count; i++)
                {
                    var itemRect = new Rectangle(Rect.X, Rect.Y + Rect.Height + i * ItemHeight, Rect.Width, ItemHeight);
                    if (itemRect.Contains(p) && leftDown)
                    {
                        SelectedIndex = i;
                        OnSelectionChanged?.Invoke(i, Options[i]);
                        IsOpen = false;
                        return;
                    }
                }

                // click outside closes
                var openRect = new Rectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height + Options.Count * ItemHeight);
                if (!openRect.Contains(p) && leftDown) IsOpen = false;
            }
        }

        public override void Draw(Renderer2D renderer, SpriteFont font)
        {
            if (!Visible) return;
            var tex = UIHelpers.GetWhiteTexture(renderer);
            // main box
            renderer.Draw(tex, new Microsoft.Xna.Framework.Vector2(Rect.X, Rect.Y), null, Background, 0f, Microsoft.Xna.Framework.Vector2.Zero, new Microsoft.Xna.Framework.Vector2(Rect.Width, Rect.Height));
            // selected text
            var label = SelectedIndex >= 0 && SelectedIndex < Options.Count ? Options[SelectedIndex] : "<select>";
            if (font != null) renderer.DrawString(font, label, new Microsoft.Xna.Framework.Vector2(Rect.X + 6, Rect.Y + (Rect.Height - font.LineSpacing) / 2f), TextColor);

            if (IsOpen)
            {
                for (int i = 0; i < Options.Count; i++)
                {
                    var itemRect = new Rectangle(Rect.X, Rect.Y + Rect.Height + i * ItemHeight, Rect.Width, ItemHeight);
                    renderer.Draw(tex, new Microsoft.Xna.Framework.Vector2(itemRect.X, itemRect.Y), null, Background, 0f, Microsoft.Xna.Framework.Vector2.Zero, new Microsoft.Xna.Framework.Vector2(itemRect.Width, itemRect.Height));
                    if (font != null) renderer.DrawString(font, Options[i], new Microsoft.Xna.Framework.Vector2(itemRect.X + 6, itemRect.Y + (itemRect.Height - font.LineSpacing) / 2f), TextColor);
                }
            }
        }
    }
}
