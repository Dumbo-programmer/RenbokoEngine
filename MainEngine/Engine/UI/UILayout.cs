using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenbokoEngine.Graphics;

namespace RenbokoEngine.UI
{
    public class UILayout : UIElement
    {
        public enum Direction { Horizontal, Vertical }
        public Direction LayoutDirection { get; set; } = Direction.Vertical;
        public int Spacing { get; set; } = 8;
        public Padding Padding { get; set; } = new Padding(4);
        public List<UIElement> Children { get; } = new();

        public void AddChild(UIElement child)
        {
            child.SetParent(this);
            Children.Add(child);
        }

        public override void Update()
        {
            int offset = Padding.Top;

            if (LayoutDirection == Direction.Vertical)
            {
                foreach (var c in Children)
                {
                    c.Rect = new Rectangle(Rect.X + Padding.Left, Rect.Y + offset, Rect.Width - Padding.Horizontal, c.Rect.Height);
                    offset += c.Rect.Height + Spacing;
                    c.Update();
                }
            }
            else
            {
                int childWidth = (Rect.Width - Padding.Horizontal - (Children.Count - 1) * Spacing) / Children.Count;
                for (int i = 0; i < Children.Count; i++)
                {
                    var c = Children[i];
                    int xOff = Rect.X + Padding.Left + i * (childWidth + Spacing);
                    c.Rect = new Rectangle(xOff, Rect.Y + Padding.Top, childWidth, Rect.Height - Padding.Vertical);
                    c.Update();
                }
            }
        }

        public override void Draw(Renderer2D renderer, SpriteFont font)
        {
            if (!Visible) return;
            foreach (var c in Children)
                c.Draw(renderer, font);
        }
    }

    public struct Padding
    {
        public int Left, Top, Right, Bottom;
        public Padding(int all) { Left = Top = Right = Bottom = all; }
        public Padding(int l, int t, int r, int b) { Left = l; Top = t; Right = r; Bottom = b; }
        public int Horizontal => Left + Right;
        public int Vertical => Top + Bottom;
    }
}
