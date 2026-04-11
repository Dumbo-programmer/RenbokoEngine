using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenbokoEngine.Assets;
using RenbokoEngine.Audio;
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
        private Texture2D? _pixel;
        private Texture2D? _circle;
        private Texture2D? _diamond;
        private Texture2D? _hex;
        private float _anim;

        private const int ScreenWidth = 1280;
        private const int ScreenHeight = 720;

        protected override void Start()
        {
            try { File.AppendAllText("renboko_debug.log", "MainMenuScene.Start called\n"); } catch { }
            // Try to load a font for the menu
            try { _font = ServiceLocator.Get<Renderer2D>().LoadFont("DefaultFont"); try { File.AppendAllText("renboko_debug.log", "Font loaded\n"); } catch { } } catch { _font = null; try { File.AppendAllText("renboko_debug.log", "Font missing\n"); } catch { } }

            _pixel = AssetManager.AcquireBuiltinTexture(BuiltinTextureShape.Pixel, 1);
            _circle = AssetManager.AcquireBuiltinTexture(BuiltinTextureShape.Circle, 160);
            _diamond = AssetManager.AcquireBuiltinTexture(BuiltinTextureShape.Diamond, 84);
            _hex = AssetManager.AcquireBuiltinTexture(BuiltinTextureShape.Hexagon, 92);

            // Centered start button
            int btnWidth = 260, btnHeight = 52;
            int cx = 1280 / 2 - btnWidth / 2;

            var start = new UIButton
            {
                Rect = new Rectangle(cx, 296, btnWidth, btnHeight),
                Text = "Start (Normal)",
                Background = new Color(32, 56, 52),
                HoverBackground = new Color(44, 86, 78),
                TextColor = new Color(236, 255, 244)
            };
            start.OnClick = () =>
            {
                AudioSystem.PlayProceduralOneShot(ProceduralSfxPresets.UiConfirm());
                try { File.AppendAllText("renboko_debug.log", "Start normal clicked\n"); } catch { }
                SceneManager.Load(new GameScene(GameDifficulty.Normal));
            };
            _ui.Add(start);

            var easy = new UIButton
            {
                Rect = new Rectangle(cx, 356, btnWidth, btnHeight),
                Text = "Easy",
                Background = new Color(36, 52, 76),
                HoverBackground = new Color(52, 76, 112),
                TextColor = new Color(234, 242, 255)
            };
            easy.OnClick = () =>
            {
                AudioSystem.PlayProceduralOneShot(ProceduralSfxPresets.UiConfirm());
                try { File.AppendAllText("renboko_debug.log", "Easy clicked\n"); } catch { }
                SceneManager.Load(new GameScene(GameDifficulty.Easy));
            };
            _ui.Add(easy);

            var hard = new UIButton
            {
                Rect = new Rectangle(cx, 416, btnWidth, btnHeight),
                Text = "Hard",
                Background = new Color(78, 36, 44),
                HoverBackground = new Color(108, 50, 60),
                TextColor = new Color(255, 235, 240)
            };
            hard.OnClick = () =>
            {
                AudioSystem.PlayProceduralOneShot(ProceduralSfxPresets.UiConfirm());
                try { File.AppendAllText("renboko_debug.log", "Hard clicked\n"); } catch { }
                SceneManager.Load(new GameScene(GameDifficulty.Hard));
            };
            _ui.Add(hard);

            var showcase = new UIButton
            {
                Rect = new Rectangle(cx, 476, btnWidth, btnHeight),
                Text = "Showcase",
                Background = new Color(52, 42, 84),
                HoverBackground = new Color(78, 62, 120),
                TextColor = new Color(246, 240, 255)
            };
            showcase.OnClick = () =>
            {
                AudioSystem.PlayProceduralOneShot(ProceduralSfxPresets.UiConfirm());
                try { File.AppendAllText("renboko_debug.log", "Showcase button clicked\n"); } catch { }
                SceneManager.Load(new ShowcaseScene());
            };
            _ui.Add(showcase);
        }

        public override void Update()
        {
            var time = ServiceLocator.Get<Time>();
            _anim += (float)time.DeltaTime;
            foreach (var u in _ui) u.Update();
        }

        public override void Render(Renderer2D renderer)
        {
            if (_pixel != null)
            {
                // Smooth atmospheric vertical gradient.
                var top = new Color(13, 16, 34);
                var mid = new Color(21, 30, 58);
                var bottom = new Color(34, 50, 84);

                for (int y = 0; y < ScreenHeight; y++)
                {
                    float t = (float)y / (ScreenHeight - 1f);
                    var c = t < 0.55f ? Color.Lerp(top, mid, t / 0.55f) : Color.Lerp(mid, bottom, (t - 0.55f) / 0.45f);
                    renderer.Draw(_pixel, new Vector2(0, y), null, c, 0f, Vector2.Zero, new Vector2(ScreenWidth, 1f));
                }

                // Subtle HUD panel to anchor menu controls.
                renderer.Draw(_pixel, new Vector2(ScreenWidth / 2f - 210f, 250f), null, new Color(8, 10, 20, 170), 0f, Vector2.Zero, new Vector2(420f, 315f));
            }

            // Animated geometric accents.
            if (_circle != null)
            {
                float x1 = 140f + MathF.Sin(_anim * 0.7f) * 42f;
                float y1 = 120f + MathF.Cos(_anim * 0.9f) * 26f;
                renderer.Draw(_circle, new Vector2(x1, y1), null, new Color(80, 150, 255, 40), 0f, Vector2.Zero, new Vector2(1.15f, 1.15f));

                float x2 = 930f + MathF.Cos(_anim * 0.62f) * 36f;
                float y2 = 420f + MathF.Sin(_anim * 0.76f) * 30f;
                renderer.Draw(_circle, new Vector2(x2, y2), null, new Color(175, 120, 255, 34), 0f, Vector2.Zero, new Vector2(0.84f, 0.84f));
            }

            if (_diamond != null)
            {
                renderer.Draw(_diamond, new Vector2(220, 470), null, new Color(130, 255, 220, 85), _anim * 0.8f, new Vector2(_diamond.Width / 2f, _diamond.Height / 2f), Vector2.One);
            }

            if (_hex != null)
            {
                renderer.Draw(_hex, new Vector2(1020, 210), null, new Color(255, 215, 120, 90), -_anim * 0.55f, new Vector2(_hex.Width / 2f, _hex.Height / 2f), Vector2.One);
            }

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

            var title = "Block jumper";
            var size = font.MeasureString(title);
            var titlePos = new Vector2(ScreenWidth / 2f - size.X / 2f, 176);
            renderer.DrawString(font, title, titlePos + new Vector2(2, 2), new Color(0, 0, 0, 180));
            renderer.DrawString(font, title, titlePos, new Color(235, 248, 255));

            var subtitle = "One-button rhythm runner built on Renboko Engine";
            var subSize = font.MeasureString(subtitle);
            renderer.DrawString(font, subtitle, new Vector2(ScreenWidth / 2f - subSize.X / 2f, 222), new Color(188, 205, 230));

            foreach (var u in _ui) u.Draw(renderer, font);
        }

        public override void OnUnload()
        {
            AssetManager.ReleaseBuiltinTexture(BuiltinTextureShape.Pixel, 1);
            AssetManager.ReleaseBuiltinTexture(BuiltinTextureShape.Circle, 160);
            AssetManager.ReleaseBuiltinTexture(BuiltinTextureShape.Diamond, 84);
            AssetManager.ReleaseBuiltinTexture(BuiltinTextureShape.Hexagon, 92);
            _pixel = null;
            _circle = null;
            _diamond = null;
            _hex = null;
        }
    }
}
