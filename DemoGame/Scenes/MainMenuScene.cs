using System;
using System.Collections.Generic;
using System.IO;
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
        private bool _renderLogged = false;

        protected override void Start()
        {
            try { File.AppendAllText("renboko_debug.log", "MainMenuScene.Start called\n"); } catch { }
            // Try to load a font for the menu
            try { _font = ServiceLocator.Get<Renderer2D>().LoadFont("DefaultFont"); try { File.AppendAllText("renboko_debug.log", "Font loaded\n"); } catch { } } catch { _font = null; try { File.AppendAllText("renboko_debug.log", "Font missing\n"); } catch { } }

            // Centered start button
            int btnWidth = 220, btnHeight = 48;
            int cx = 1280 / 2 - btnWidth / 2;

            var start = new UIButton { Rect = new Rectangle(cx, 300, btnWidth, btnHeight), Text = "Start Game" };
            start.OnClick = () => { try { File.AppendAllText("renboko_debug.log", "Start button clicked\n"); } catch { } SceneManager.Load(new GameScene()); };
            _ui.Add(start);

            var showcase = new UIButton { Rect = new Rectangle(cx, 360, btnWidth, btnHeight), Text = "Showcase" };
            showcase.OnClick = () => { try { File.AppendAllText("renboko_debug.log", "Showcase button clicked\n"); } catch { } SceneManager.Load(new ShowcaseScene()); };
            _ui.Add(showcase);
        }

        public override void Update()
        {
            foreach (var u in _ui) u.Update();
        }

        public override void Render(Renderer2D renderer)
        {
            // Debug: attempt to draw a known texture to confirm rendering works
            try
            {
                var dbgTex = RenbokoEngine.Assets.AssetManager.AcquireTexture("DemoGame/Content/player.png");
                if (dbgTex != null)
                {
                    renderer.Draw(dbgTex, new Vector2(20, 20), null, Color.White, 0f, Vector2.Zero, new Vector2(2f, 2f));
                }
            }
            catch { }

            // Draw title
            SpriteFont? font = _font;
            if (font == null)
            {
                try { font = renderer.LoadFont("DefaultFont"); } catch { font = null; }
            }

            if (!_renderLogged)
            {
                try { File.AppendAllText("renboko_debug.log", "MainMenuScene.Render called\n"); } catch { }
                _renderLogged = true;
            }

            if (font == null) return;

            var title = "Renboko Demo";
            var size = font.MeasureString(title);
            renderer.DrawString(font, title, new Vector2(1280 / 2f - size.X / 2f, 200), Color.White);

            foreach (var u in _ui) u.Draw(renderer, font);
        }
    }
}
