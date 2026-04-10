using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenbokoEngine.Scenes;
using RenbokoEngine.UI;
using RenbokoEngine.Core;
using RenbokoEngine.Graphics;

namespace DemoGame
{
    public class MainMenuScene : Scene
    {
        private readonly List<UIElement> _ui = new();
        private SpriteFont? _font;

        protected override void Start()
        {
            // Try to load a font for the menu
            try { _font = ServiceLocator.Get<Renderer2D>().LoadFont("DefaultFont"); } catch { _font = null; }

            // Centered start button
            int btnWidth = 220, btnHeight = 48;
            int cx = 1280 / 2 - btnWidth / 2;

            var start = new UIButton { Rect = new Rectangle(cx, 300, btnWidth, btnHeight), Text = "Start Game" };
            start.OnClick = () => SceneManager.Load(new GameScene());
            _ui.Add(start);

            var showcase = new UIButton { Rect = new Rectangle(cx, 360, btnWidth, btnHeight), Text = "Showcase" };
            showcase.OnClick = () => SceneManager.Load(new ShowcaseScene());
            _ui.Add(showcase);
        }

        public override void Update()
        {
            foreach (var u in _ui) u.Update();
        }

        public override void Render(Renderer2D renderer)
        {
            // Draw title
            SpriteFont? font = _font;
            if (font == null)
            {
                try { font = renderer.LoadFont("DefaultFont"); } catch { font = null; }
            }

            if (font != null)
            {
                var title = "Renboko Demo";
                var size = font.MeasureString(title);
                renderer.DrawString(font, title, new Vector2(1280 / 2f - size.X / 2f, 200), Color.White);
            }

            foreach (var u in _ui) u.Draw(renderer, font);
        }
    }
}
