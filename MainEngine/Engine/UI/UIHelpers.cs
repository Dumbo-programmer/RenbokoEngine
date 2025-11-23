using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using RenbokoEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
namespace RenbokoEngine.UI
{
    internal static class UIHelpers
    {
        private static Texture2D? _white;

    // This just gives you a 1x1 white texture (super handy for drawing rectangles and stuff).
    // It grabs the GraphicsDevice from Renderer2D using reflection (yeah, it's a bit hacky, but it works).
        public static Texture2D GetWhiteTexture(Renderer2D r)
        {
            if (_white != null) return _white;

            // try to get GraphicsDevice via reflection
            var gdField = typeof(Renderer2D).GetField("_gd", BindingFlags.NonPublic | BindingFlags.Instance);
            GraphicsDevice gd;
            if (gdField != null)
            {
                gd = (GraphicsDevice)gdField.GetValue(r)!;
            }
            else
            {
        // If this ever runs, something's really wrong. You probably broke Renderer2D.
        throw new InvalidOperationException("Renderer2D must expose GraphicsDevice via private field _gd for UIHelpers to work.");
            }

            _white = new Texture2D(gd, 1, 1);
            _white.SetData(new[] { Color.White });
            return _white;
        }
    }
}
