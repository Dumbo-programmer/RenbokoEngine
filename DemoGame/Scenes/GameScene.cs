using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RenbokoEngine.Scenes;
using RenbokoEngine.Graphics;
using RenbokoEngine.Core;

namespace DemoGame
{
    // A small, self-contained endless-runner implementation using existing assets.
    public class GameScene : Scene
    {
        private Sprite? _playerSprite;
        private Vector2 _playerPos;
        private float _verticalVelocity;
        private bool _isGrounded;

        private readonly List<Vector2> _obstacles = new();
        private Sprite? _obstacleSprite;

        private float _distanceUntilNextSpawn;
        private readonly Random _rng = new();

        private int _screenWidth = 1280;
        private int _screenHeight = 720;
        // _floorY is the y-coordinate of the ground baseline (where sprites' bottoms should rest)
        private float _floorY = 500f;

        private float _score = 0f;
        private bool _isGameOver = false;

        // Tunables
        private const float RunSpeed = 320f; // world scroll speed (px/s)
        private const float Gravity = 1200f; // px/s^2 (reduced to allow higher, longer jumps)
        private const float JumpVelocity = -900f; // initial jump velocity (stronger jump)

        protected override void Start()
        {
            // Load sprites
            var playerTex = RenbokoEngine.Assets.AssetManager.AcquireTexture("DemoGame/Content/player.png");
            _playerSprite = new Sprite(playerTex);
            _obstacleSprite = new Sprite(RenbokoEngine.Assets.AssetManager.AcquireTexture("DemoGame/Content/coin.png"));

            // Screen / ground setup
            _screenWidth = 1280;
            _screenHeight = 720;

            // Ground baseline: position measured in pixels from top of screen.
            // Increase the bottom margin to raise the standing line.
            int bottomMargin = 120;
            _floorY = _screenHeight - bottomMargin;

            // Player start (top-left Y so bottom aligns with _floorY)
            int playerH = _playerSprite?.Texture.Height ?? 32;
            _playerPos = new Vector2(200, _floorY - playerH);

            // Debug: record computed floor and player start position
            try { File.AppendAllText("renboko_debug.log", $"GameScene.Start floorY={_floorY}, playerStartY={_playerPos.Y}\n"); } catch { }
            _verticalVelocity = 0f;
            _isGrounded = true;

            // Spawn system: distance-based so spacing is deterministic and fair.
            _obstacles.Clear();
            _distanceUntilNextSpawn = NextSpawnGapDistance();
            _score = 0f;
            _isGameOver = false;
        }

        public override void Update()
        {
            var input = ServiceLocator.Get<RenbokoEngine.Input.InputSystem>();
            var keyboard = input.GetDevice<RenbokoEngine.Input.KeyboardDevice>();
            var time = ServiceLocator.Get<Time>();
            if (time == null) return;

            float dt = (float)time.DeltaTime;

            if (_isGameOver)
            {
                // Restart on Enter
                if (keyboard?.GetKeyDown(Keys.Enter) == true || keyboard?.GetKeyDown(Keys.Space) == true)
                    SceneManager.Load(new GameScene());
                return;
            }

            // Jump
            if (_isGrounded && keyboard != null && (keyboard.GetKeyDown(Keys.Space) || keyboard.GetKeyDown(Keys.Up)))
            {
                _verticalVelocity = JumpVelocity;
                _isGrounded = false;
            }

            // Apply gravity
            _verticalVelocity += Gravity * dt;
            _playerPos.Y += _verticalVelocity * dt;
            int playerH = _playerSprite?.Texture.Height ?? 32;
            if (_playerPos.Y + playerH >= _floorY)
            {
                _playerPos.Y = _floorY - playerH;
                _verticalVelocity = 0f;
                _isGrounded = true;
            }

            // Move obstacles (world scroll)
            for (int i = _obstacles.Count - 1; i >= 0; i--)
            {
                var p = _obstacles[i];
                p.X -= RunSpeed * dt;
                _obstacles[i] = p;
                if (p.X < -200) _obstacles.RemoveAt(i);
            }

            // Spawn logic (distance-based): guarantees enough room to jump and land.
            _distanceUntilNextSpawn -= RunSpeed * dt;
            if (_distanceUntilNextSpawn <= 0f)
            {
                float spawnX = _screenWidth + _rng.Next(50, 220);
                int obsH = _obstacleSprite?.Texture.Height ?? 16;
                var obstaclePos = new Vector2(spawnX, _floorY - obsH);
                _obstacles.Add(obstaclePos);
                _distanceUntilNextSpawn = NextSpawnGapDistance();
            }

            // Collision check (AABB)
            if (_playerSprite != null && _obstacleSprite != null)
            {
                var playerRect = new Rectangle((int)_playerPos.X, (int)_playerPos.Y, _playerSprite.Texture.Width, _playerSprite.Texture.Height);
                foreach (var o in _obstacles)
                {
                    var obsRect = new Rectangle((int)o.X, (int)o.Y, _obstacleSprite.Texture.Width, _obstacleSprite.Texture.Height);
                    if (playerRect.Intersects(obsRect))
                    {
                        _isGameOver = true;
                        break;
                    }
                }
            }

            // Update score (distance-based)
            _score += RunSpeed * dt * 0.1f;
        }

        public override void Render(Renderer2D renderer)
        {
            // Draw player and obstacles
            if (_playerSprite != null) _playerSprite.Draw(renderer, _playerPos);
            if (_obstacleSprite != null)
            {
                foreach (var o in _obstacles) _obstacleSprite.Draw(renderer, o);
            }

            // HUD: score and game over
            SpriteFont? font = null;
            try { font = renderer.LoadFont("DefaultFont"); } catch { font = null; }

            if (font != null)
            {
                renderer.DrawString(font, $"Score: {(int)_score}", new Vector2(16, 16), Microsoft.Xna.Framework.Color.White);
                if (_isGameOver)
                {
                    var msg = "GAME OVER - Press Enter to Restart";
                    var size = font.MeasureString(msg);
                    renderer.DrawString(font, msg, new Vector2(_screenWidth / 2f - size.X / 2f, _screenHeight / 2f - size.Y / 2f), Microsoft.Xna.Framework.Color.Yellow);
                }
            }
        }

        private float NextSpawnGapDistance()
        {
            // Air time for a simple jump under constant gravity:
            // t_air = (2 * |v0|) / g
            float airTime = (2f * MathF.Abs(JumpVelocity)) / Gravity;
            float jumpTravel = RunSpeed * airTime;

            // Require a minimum post-jump landing buffer before next obstacle.
            const float landingBufferPx = 260f;
            const float randomExtraPx = 220f;

            return jumpTravel + landingBufferPx + (float)_rng.NextDouble() * randomExtraPx;
        }
    }
}
