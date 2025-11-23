using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenbokoEngine.Graphics;

namespace RenbokoEngine.UI
{
    // This is the base class for all UI stuff. Position and size are in pixels (screen space).
    // If you want to make a button or label or whatever, inherit from this!
    public abstract class UIElement
    {
        public Rectangle Rect;
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;

        public UIElement? Parent { get; private set; }

        public void SetParent(UIElement parent) => Parent = parent;

    // This gets called every frame so you can update input or state.
        public virtual void Update() { }

    // You gotta implement this to draw your UI element. The font comes from the scene.
        public abstract void Draw(Renderer2D renderer, SpriteFont font);

    // Checks if a point (like the mouse) is inside this element (takes parent position into account).
        public bool ContainsPoint(Point p)
        {
            var absPos = GetAbsoluteRect();
            return absPos.Contains(p);
        }

    // Gets the real position of this element, including all parent offsets. Super useful for nested UI.
        public Rectangle GetAbsoluteRect()
        {
            if (Parent == null) return Rect;
            var parentRect = Parent.GetAbsoluteRect();
            return new Rectangle(Rect.X + parentRect.X, Rect.Y + parentRect.Y, Rect.Width, Rect.Height);
        }
    }
}
