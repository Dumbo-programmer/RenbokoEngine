using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenbokoEngine.Scenes;
using RenbokoEngine.Physics;
using RenbokoEngine.Graphics;
using RenbokoEngine.Core;
using RenbokoEngine.UI;

namespace DemoGame
{
    // A simple showcase scene that spawns a few physics boxes and exposes a UI button to spawn more.
    public class ShowcaseScene : Scene
    {
        private readonly List<(Rigidbody2D Body, Sprite? Sprite)> _entities = new();
        private readonly List<UIElement> _ui = new();
        private SpriteFont? _font;
        private PhysicsWorld? _physics;
        private Renderer2D? _renderer;
        private int _spawnIndex = 0;

        protected override void Start()
        {
            _renderer = ServiceLocator.Get<Renderer2D>();
            _physics = ServiceLocator.Get<PhysicsWorld>();

            try { _font = _renderer.LoadFont("DefaultFont"); } catch { _font = null; }

            // Floor
            var floor = new Rigidbody2D(new BoxCollider2D(new Vector2(800, 40))) { IsStatic = true, Position = new Vector2(400, 540) };
            _physics.AddBody(floor);

            // UI: spawn button
            var spawnBtn = new UIButton() { Rect = new Microsoft.Xna.Framework.Rectangle(20, 20, 160, 40), Text = "Spawn Box" };
            spawnBtn.OnClick = () => SpawnBox();
            _ui.Add(spawnBtn);

            // Spawn a few initial boxes
            for (int i = 0; i < 3; i++) SpawnBox();
        }

        private void SpawnBox()
        {
            var collider = new BoxCollider2D(new Vector2(32, 32));
            var rb = new Rigidbody2D(collider) { Position = new Vector2(200 + _spawnIndex * 40, 50), Mass = 1f };
            _physics?.AddBody(rb);

            Sprite? sprite = null;
            try
            {
                var tex = RenbokoEngine.Assets.AssetManager.AcquireTexture("DemoGame/Content/player.png");
                sprite = new Sprite(tex);
            }
            catch { /* ignore missing texture; we'll still show physics */ }

            _entities.Add((rb, sprite));
            _spawnIndex++;
        }

        public override void Update()
        {
            foreach (var u in _ui) u.Update();
        }

        public override void Render(Renderer2D renderer)
        {
            // Draw physics entities
            foreach (var e in _entities)
            {
                if (e.Sprite != null)
                {
                    e.Sprite.Draw(renderer, new Vector2(e.Body.Position.X, e.Body.Position.Y));
                }
                else
                {
                    // Attempt to draw a fallback texture if available
                    try
                    {
                        var tex = RenbokoEngine.Assets.AssetManager.AcquireTexture("DemoGame/Content/player.png");
                        renderer.Draw(tex, new Vector2(e.Body.Position.X, e.Body.Position.Y), null, Color.White, 0f, Vector2.Zero, Vector2.One);
                    }
                    catch { }
                }
            }

            // Draw UI (use font if available)
            foreach (var u in _ui) u.Draw(renderer, _font ?? _renderer!.LoadFont("DefaultFont"));
        }
    }
}
