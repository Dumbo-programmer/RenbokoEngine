using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RenbokoEngine.Assets;
using RenbokoEngine.Scenes;
using RenbokoEngine.Graphics;
using RenbokoEngine.Core;

namespace DemoGame
{
    public enum GameDifficulty
    {
        Easy,
        Normal,
        Hard
    }

    // Endless-runner scene that uses only built-in primitive textures.
    public class GameScene : Scene
    {
        private enum ObstacleKind
        {
            Spike,
            Block,
            Orb,
            Crystal,
            Saw
        }

        private sealed class Obstacle
        {
            public ObstacleKind Kind;
            public Vector2 Position;
            public int Size;
            public float BobPhase;
            public float Rotation;
            public float RotationSpeed;
        }

        private sealed class TrailPoint
        {
            public Vector2 Position;
            public float Life;
        }

        private sealed class JumpPad
        {
            public Vector2 Position;
            public float Cooldown;
        }

        private Sprite? _playerSprite;
        private Vector2 _playerPos;
        private float _verticalVelocity;
        private bool _isGrounded;
        private float _elapsed;

        private readonly List<Obstacle> _obstacles = new();
        private readonly List<Vector2> _pickups = new();
        private readonly List<TrailPoint> _trail = new();
        private readonly List<JumpPad> _jumpPads = new();

        private Texture2D? _groundPixel;
        private Texture2D? _triangleTex;
        private Texture2D? _squareTex;
        private Texture2D? _circleTex;
        private Texture2D? _diamondTex;
        private Texture2D? _hexTex;
        private Texture2D? _sawTex;
        private Texture2D? _pickupTex;

        private float _distanceUntilNextSpawn;
        private float _distanceUntilNextPickup;
        private float _distanceUntilNextPad;
        private float _trailSpawnTimer;
        private readonly Random _rng = new();

        private int _screenWidth = 1280;
        private int _screenHeight = 720;
        private float _floorY = 500f;

        private float _score = 0f;
        private bool _isGameOver = false;
        private bool _isPaused = false;
        private bool _nightMode = false;
        private float _nightBlend = 0f;
        private float _nextTwistScore = 30f;
        private float _slowMoTimer = 0f;

        private readonly GameDifficulty _difficulty;
        private static int _attemptCounter = 0;
        private int _attemptNumber;

        private const float TrailLifetime = 0.42f;
        private const float TrailSpawnInterval = 0.03f;

        // Difficulty config
        private float _baseRunSpeed;
        private float _gravity;
        private float _jumpVelocity;
        private float _difficultyRamp;
        private float _landingBuffer;
        private float _clusterChance;

        public GameScene(GameDifficulty difficulty = GameDifficulty.Normal)
        {
            _difficulty = difficulty;
            ConfigureDifficulty(difficulty);
        }

        private void ConfigureDifficulty(GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case GameDifficulty.Easy:
                    _baseRunSpeed = 270f;
                    _gravity = 1080f;
                    _jumpVelocity = -920f;
                    _difficultyRamp = 0.015f;
                    _landingBuffer = 380f;
                    _clusterChance = 0.08f;
                    break;
                case GameDifficulty.Hard:
                    _baseRunSpeed = 370f;
                    _gravity = 1320f;
                    _jumpVelocity = -950f;
                    _difficultyRamp = 0.028f;
                    _landingBuffer = 220f;
                    _clusterChance = 0.24f;
                    break;
                default:
                    _baseRunSpeed = 320f;
                    _gravity = 1200f;
                    _jumpVelocity = -930f;
                    _difficultyRamp = 0.02f;
                    _landingBuffer = 290f;
                    _clusterChance = 0.15f;
                    break;
            }
        }

        private float CurrentRunSpeed
        {
            get
            {
                float scaled = _baseRunSpeed * (1f + _score * _difficultyRamp * 0.01f);
                float capped = MathF.Min(_baseRunSpeed * 1.9f, scaled);
                if (_slowMoTimer > 0f) capped *= 0.6f;
                return capped;
            }
        }

        protected override void Start()
        {
            // Built-in primitives so no external textures are required.
            _playerSprite = new Sprite(AssetManager.AcquireBuiltinTexture(BuiltinTextureShape.Cube, 48))
            {
                Color = Color.LimeGreen
            };

            _triangleTex = AssetManager.AcquireBuiltinTexture(BuiltinTextureShape.Triangle, 44);
            _squareTex = AssetManager.AcquireBuiltinTexture(BuiltinTextureShape.Square, 46);
            _circleTex = AssetManager.AcquireBuiltinTexture(BuiltinTextureShape.Circle, 40);
            _diamondTex = AssetManager.AcquireBuiltinTexture(BuiltinTextureShape.Diamond, 44);
            _hexTex = AssetManager.AcquireBuiltinTexture(BuiltinTextureShape.Hexagon, 38);
            _sawTex = AssetManager.AcquireBuiltinTexture(BuiltinTextureShape.Saw, 56);
            _pickupTex = AssetManager.AcquireBuiltinTexture(BuiltinTextureShape.Star, 24);
            _groundPixel = AssetManager.AcquireBuiltinTexture(BuiltinTextureShape.Pixel, 1);

            _screenWidth = 1280;
            _screenHeight = 720;

            int bottomMargin = 120;
            _floorY = _screenHeight - bottomMargin;

            int playerH = _playerSprite?.Texture.Height ?? 32;
            _playerPos = new Vector2(200, _floorY - playerH);

            _attemptCounter++;
            _attemptNumber = _attemptCounter;

            try { File.AppendAllText("renboko_debug.log", $"GameScene.Start diff={_difficulty}, floorY={_floorY}\n"); } catch { }

            _verticalVelocity = 0f;
            _isGrounded = true;
            _elapsed = 0f;

            _obstacles.Clear();
            _pickups.Clear();
            _trail.Clear();
            _jumpPads.Clear();
            _distanceUntilNextSpawn = NextSpawnGapDistance();
            _distanceUntilNextPickup = 1600f;
            _distanceUntilNextPad = 1500f;
            _trailSpawnTimer = 0f;
            _score = 0f;
            _isGameOver = false;
            _isPaused = false;
            _nightMode = false;
            _nightBlend = 0f;
            _nextTwistScore = 30f;
            _slowMoTimer = 0f;
        }

        public override void Update()
        {
            var input = ServiceLocator.Get<RenbokoEngine.Input.InputSystem>();
            var keyboard = input.GetDevice<RenbokoEngine.Input.KeyboardDevice>();
            var time = ServiceLocator.Get<Time>();

            // Global controls
            if (keyboard?.GetKeyDown(Keys.Escape) == true)
            {
                SceneManager.Load(new MainMenuScene());
                return;
            }

            if (!_isGameOver && keyboard?.GetKeyDown(Keys.P) == true)
            {
                _isPaused = !_isPaused;
            }

            if (_isPaused)
            {
                return;
            }

            float dt = (float)time.DeltaTime;
            _elapsed += dt;
            if (_slowMoTimer > 0f) _slowMoTimer -= dt;

            float targetNight = _nightMode ? 1f : 0f;
            _nightBlend = MathHelper.Lerp(_nightBlend, targetNight, dt * 2.4f);

            if (_isGameOver)
            {
                if (keyboard?.GetKeyDown(Keys.Enter) == true || keyboard?.GetKeyDown(Keys.Space) == true)
                    SceneManager.Load(new GameScene(_difficulty));
                return;
            }

            if (_isGrounded && keyboard != null && (keyboard.GetKeyDown(Keys.Space) || keyboard.GetKeyDown(Keys.Up)))
            {
                _verticalVelocity = _jumpVelocity;
                _isGrounded = false;
            }

            _verticalVelocity += _gravity * dt;
            _playerPos.Y += _verticalVelocity * dt;
            int playerH = _playerSprite?.Texture.Height ?? 32;
            if (_playerPos.Y + playerH >= _floorY)
            {
                _playerPos.Y = _floorY - playerH;
                _verticalVelocity = 0f;
                _isGrounded = true;
            }

            if (_playerSprite != null)
            {
                if (!_isGrounded)
                {
                    _playerSprite.Rotation += dt * 9.5f;
                }
                else
                {
                    _playerSprite.Rotation = MathHelper.Lerp(_playerSprite.Rotation, 0f, dt * 22f);
                    if (MathF.Abs(_playerSprite.Rotation) < 0.02f) _playerSprite.Rotation = 0f;
                }
            }

            // Update player trail (kept lightweight and only emitted while airborne).
            for (int i = _trail.Count - 1; i >= 0; i--)
            {
                _trail[i].Life -= dt;
                if (_trail[i].Life <= 0f)
                {
                    _trail.RemoveAt(i);
                }
            }

            _trailSpawnTimer -= dt;
            if (_trailSpawnTimer <= 0f && !_isGrounded)
            {
                _trail.Add(new TrailPoint { Position = _playerPos, Life = TrailLifetime });
                _trailSpawnTimer = TrailSpawnInterval;
            }

            float runSpeed = CurrentRunSpeed;

            for (int i = _obstacles.Count - 1; i >= 0; i--)
            {
                var obstacle = _obstacles[i];
                obstacle.Position.X -= runSpeed * dt;
                obstacle.Rotation += obstacle.RotationSpeed * dt;

                if (obstacle.Kind == ObstacleKind.Orb)
                {
                    float baseY = (_floorY - obstacle.Size) - 130f;
                    obstacle.Position.Y = baseY + MathF.Sin((_elapsed * 4f) + obstacle.BobPhase) * 35f;
                }

                if (obstacle.Kind == ObstacleKind.Crystal)
                {
                    float baseY = (_floorY - obstacle.Size) - 70f;
                    obstacle.Position.Y = baseY + MathF.Sin((_elapsed * 3.2f) + obstacle.BobPhase) * 24f;
                }

                if (obstacle.Position.X < -220)
                {
                    _obstacles.RemoveAt(i);
                }
            }

            for (int i = _jumpPads.Count - 1; i >= 0; i--)
            {
                var pad = _jumpPads[i];
                pad.Position.X -= runSpeed * dt;
                pad.Cooldown = MathF.Max(0f, pad.Cooldown - dt);
                if (pad.Position.X < -120)
                {
                    _jumpPads.RemoveAt(i);
                }
            }

            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                var p = _pickups[i];
                p.X -= runSpeed * dt;
                _pickups[i] = p;
                if (p.X < -60) _pickups.RemoveAt(i);
            }

            _distanceUntilNextSpawn -= runSpeed * dt;
            if (_distanceUntilNextSpawn <= 0f)
            {
                SpawnObstacleSet();
                _distanceUntilNextSpawn = NextSpawnGapDistance();
            }

            _distanceUntilNextPickup -= runSpeed * dt;
            if (_distanceUntilNextPickup <= 0f)
            {
                float pickupX = _screenWidth + _rng.Next(180, 320);
                float pickupY = (_floorY - 24f) - _rng.Next(90, 190);
                _pickups.Add(new Vector2(pickupX, pickupY));
                _distanceUntilNextPickup = _rng.Next(1200, 2200);
            }

            _distanceUntilNextPad -= runSpeed * dt;
            if (_distanceUntilNextPad <= 0f)
            {
                float padX = _screenWidth + _rng.Next(140, 260);
                _jumpPads.Add(new JumpPad
                {
                    Position = new Vector2(padX, _floorY - 18f),
                    Cooldown = 0f
                });
                _distanceUntilNextPad = _rng.Next(1700, 2900);
            }

            if (_playerSprite != null)
            {
                var playerRect = new Rectangle((int)_playerPos.X, (int)_playerPos.Y, _playerSprite.Texture.Width, _playerSprite.Texture.Height);

                foreach (var obstacle in _obstacles)
                {
                    var obsRect = new Rectangle((int)obstacle.Position.X, (int)obstacle.Position.Y, obstacle.Size, obstacle.Size);
                    if (playerRect.Intersects(obsRect))
                    {
                        _isGameOver = true;
                        break;
                    }
                }

                for (int i = _pickups.Count - 1; i >= 0; i--)
                {
                    var pickupRect = new Rectangle((int)_pickups[i].X, (int)_pickups[i].Y, 24, 24);
                    if (playerRect.Intersects(pickupRect))
                    {
                        _slowMoTimer = 4f;
                        _pickups.RemoveAt(i);
                    }
                }

                for (int i = 0; i < _jumpPads.Count; i++)
                {
                    var padRect = new Rectangle((int)_jumpPads[i].Position.X, (int)_jumpPads[i].Position.Y, 38, 18);
                    if (_jumpPads[i].Cooldown <= 0f && playerRect.Intersects(padRect))
                    {
                        _verticalVelocity = _jumpVelocity * 1.12f;
                        _isGrounded = false;
                        _jumpPads[i].Cooldown = 0.22f;
                    }
                }
            }

            _score += runSpeed * dt * 0.1f;

            if (_score >= _nextTwistScore)
            {
                _nextTwistScore += 30f;
                _nightMode = !_nightMode;
                _clusterChance = MathF.Min(0.42f, _clusterChance + 0.03f);
            }
        }

        public override void Render(Renderer2D renderer)
        {
            if (_groundPixel != null)
            {
                // Smooth day/night vertical gradient.
                var dayTop = new Color(26, 30, 54);
                var dayBottom = new Color(66, 80, 124);
                var nightTop = new Color(10, 12, 24);
                var nightBottom = new Color(22, 26, 40);
                var top = Color.Lerp(dayTop, nightTop, _nightBlend);
                var bottom = Color.Lerp(dayBottom, nightBottom, _nightBlend);
                // Draw 1px rows for a visually continuous gradient (no visible band steps).
                const int band = 1;
                for (int y = 0; y < _screenHeight; y++)
                {
                    float t = (float)y / MathF.Max(1f, _screenHeight - 1f);
                    var c = Color.Lerp(top, bottom, t);
                    renderer.Draw(_groundPixel, new Vector2(0, y), null, c, 0f, Vector2.Zero, new Vector2(_screenWidth, band));
                }
            }

            if (_groundPixel != null)
            {
                renderer.Draw(_groundPixel, new Vector2(0, _floorY), null, Color.DimGray, 0f, Vector2.Zero, new Vector2(_screenWidth, 3f));
            }

            if (_hexTex != null)
            {
                foreach (var pad in _jumpPads)
                {
                    renderer.Draw(_hexTex, pad.Position, null, new Color(64, 220, 255), 0f, Vector2.Zero, new Vector2(1f, 0.5f));
                }
            }

            if (_playerSprite != null)
            {
                // Gradient trail behind player (older points fade and shift color).
                for (int i = 0; i < _trail.Count; i++)
                {
                    float lifeRatio = MathHelper.Clamp(_trail[i].Life / TrailLifetime, 0f, 1f);
                    float cT = 1f - lifeRatio;
                    Color start = new(255, 84, 200);
                    Color mid = new(90, 220, 255);
                    Color end = new(130, 255, 170);
                    Color grad = cT < 0.5f ? Color.Lerp(start, mid, cT * 2f) : Color.Lerp(mid, end, (cT - 0.5f) * 2f);
                    Color drawColor = grad * (lifeRatio * 0.75f);
                    float scale = 0.65f + (lifeRatio * 0.35f);

                    renderer.Draw(
                        _playerSprite.Texture,
                        _trail[i].Position,
                        null,
                        drawColor,
                        0f,
                        Vector2.Zero,
                        new Vector2(scale, scale));
                }

                _playerSprite.Draw(renderer, _playerPos);
            }

            foreach (var obstacle in _obstacles)
            {
                var tex = GetTextureForObstacle(obstacle.Kind);
                var color = GetColorForObstacle(obstacle.Kind);
                float sx = (float)obstacle.Size / tex.Width;
                float sy = (float)obstacle.Size / tex.Height;
                renderer.Draw(
                    tex,
                    obstacle.Position + new Vector2(obstacle.Size * 0.5f, obstacle.Size * 0.5f),
                    null,
                    color,
                    obstacle.Rotation,
                    new Vector2(tex.Width * 0.5f, tex.Height * 0.5f),
                    new Vector2(sx, sy));
            }

            if (_pickupTex != null)
            {
                foreach (var p in _pickups)
                {
                    float pulse = 0.9f + MathF.Sin((_elapsed * 9f) + (p.X * 0.01f)) * 0.12f;
                    renderer.Draw(_pickupTex, p + new Vector2(12, 12), null, Color.Cyan, _elapsed * 4f, new Vector2(12, 12), new Vector2(pulse, pulse));
                }
            }

            if (_isPaused && _groundPixel != null)
            {
                renderer.Draw(_groundPixel, Vector2.Zero, null, new Color(0, 0, 0, 150), 0f, Vector2.Zero, new Vector2(_screenWidth, _screenHeight));
            }

            SpriteFont? font = null;
            try { font = renderer.LoadFont("DefaultFont"); } catch { font = null; }

            if (font != null)
            {
                renderer.DrawString(font, $"Score: {(int)_score}", new Vector2(16, 16), Microsoft.Xna.Framework.Color.White);
                renderer.DrawString(font, $"Difficulty: {_difficulty}", new Vector2(16, 44), Color.White);
                renderer.DrawString(font, $"Attempt: {_attemptNumber}", new Vector2(16, 72), Color.White);
                renderer.DrawString(font, "P: Pause/Resume   Esc: Main Menu", new Vector2(16, 100), Color.LightGray);

                if (_slowMoTimer > 0f)
                {
                    renderer.DrawString(font, $"Slow-Mo: {_slowMoTimer:0.0}s", new Vector2(16, 128), Color.Cyan);
                }

                if (_isPaused)
                {
                    var msg = "PAUSED";
                    var msg2 = "Press P to Resume";
                    var msg3 = "Press Esc for Main Menu";
                    var s1 = font.MeasureString(msg);
                    var s2 = font.MeasureString(msg2);
                    var s3 = font.MeasureString(msg3);
                    renderer.DrawString(font, msg, new Vector2(_screenWidth / 2f - s1.X / 2f, _screenHeight / 2f - 60), Color.Yellow);
                    renderer.DrawString(font, msg2, new Vector2(_screenWidth / 2f - s2.X / 2f, _screenHeight / 2f - 20), Color.White);
                    renderer.DrawString(font, msg3, new Vector2(_screenWidth / 2f - s3.X / 2f, _screenHeight / 2f + 20), Color.White);
                }

                if (_isGameOver)
                {
                    var msg = "GAME OVER - Press Enter/Space to Restart";
                    var size = font.MeasureString(msg);
                    renderer.DrawString(font, msg, new Vector2(_screenWidth / 2f - size.X / 2f, _screenHeight / 2f - size.Y / 2f), Microsoft.Xna.Framework.Color.Yellow);
                }
            }
        }

        private void SpawnObstacleSet()
        {
            float spawnX = _screenWidth + _rng.Next(80, 260);
            var first = CreateRandomObstacle(spawnX);
            _obstacles.Add(first);

            // Occasional clusters create rhythm changes (GD-style pattern bursts).
            if (_rng.NextDouble() < _clusterChance)
            {
                float clusterGap = _rng.Next(110, 190);
                var second = CreateRandomObstacle(spawnX + clusterGap);
                _obstacles.Add(second);

                if (_rng.NextDouble() < _clusterChance * 0.55)
                {
                    float thirdGap = clusterGap + _rng.Next(105, 170);
                    var third = CreateRandomObstacle(spawnX + thirdGap);
                    _obstacles.Add(third);
                }
            }
        }

        private Obstacle CreateRandomObstacle(float x)
        {
            double roll = _rng.NextDouble();

            // As score increases, introduce more variety.
            if (_score < 30f)
            {
                roll *= 0.45; // mostly spikes at low score
            }

            if (roll < 0.46)
            {
                const int size = 44;
                return new Obstacle
                {
                    Kind = ObstacleKind.Spike,
                    Size = size,
                    Position = new Vector2(x, _floorY - size),
                    BobPhase = 0f,
                    Rotation = 0f,
                    RotationSpeed = 0f
                };
            }

            if (roll < 0.70)
            {
                const int size = 48;
                return new Obstacle
                {
                    Kind = ObstacleKind.Block,
                    Size = size,
                    Position = new Vector2(x, _floorY - size),
                    BobPhase = 0f,
                    Rotation = 0f,
                    RotationSpeed = 0f
                };
            }

            if (roll < 0.84)
            {
                const int orbSize = 40;
                return new Obstacle
                {
                    Kind = ObstacleKind.Orb,
                    Size = orbSize,
                    Position = new Vector2(x, (_floorY - orbSize) - 130f),
                    BobPhase = (float)_rng.NextDouble() * MathF.PI * 2f,
                    Rotation = 0f,
                    RotationSpeed = 1.2f
                };
            }

            if (roll < 0.94)
            {
                const int crystalSize = 44;
                return new Obstacle
                {
                    Kind = ObstacleKind.Crystal,
                    Size = crystalSize,
                    Position = new Vector2(x, (_floorY - crystalSize) - 70f),
                    BobPhase = (float)_rng.NextDouble() * MathF.PI * 2f,
                    Rotation = 0f,
                    RotationSpeed = 1.8f
                };
            }

            const int sawSize = 52;
            return new Obstacle
            {
                Kind = ObstacleKind.Saw,
                Size = sawSize,
                Position = new Vector2(x, (_floorY - sawSize) - 20f),
                BobPhase = 0f,
                Rotation = 0f,
                RotationSpeed = 4.8f
            };
        }

        private Texture2D GetTextureForObstacle(ObstacleKind kind)
        {
            return kind switch
            {
                ObstacleKind.Block => _squareTex ?? _triangleTex!,
                ObstacleKind.Orb => _circleTex ?? _triangleTex!,
                ObstacleKind.Crystal => _diamondTex ?? _triangleTex!,
                ObstacleKind.Saw => _sawTex ?? _triangleTex!,
                _ => _triangleTex!
            };
        }

        private static Color GetColorForObstacle(ObstacleKind kind)
        {
            return kind switch
            {
                ObstacleKind.Block => new Color(255, 160, 64),
                ObstacleKind.Orb => new Color(200, 120, 255),
                ObstacleKind.Crystal => new Color(120, 255, 225),
                ObstacleKind.Saw => new Color(255, 92, 92),
                _ => Color.OrangeRed
            };
        }

        private float NextSpawnGapDistance()
        {
            float airTime = (2f * MathF.Abs(_jumpVelocity)) / _gravity;
            float jumpTravel = CurrentRunSpeed * airTime;

            const float randomExtraPx = 220f;

            return jumpTravel + _landingBuffer + (float)_rng.NextDouble() * randomExtraPx;
        }

        public override void OnUnload()
        {
            AssetManager.ReleaseBuiltinTexture(BuiltinTextureShape.Cube, 48);
            AssetManager.ReleaseBuiltinTexture(BuiltinTextureShape.Triangle, 44);
            AssetManager.ReleaseBuiltinTexture(BuiltinTextureShape.Square, 46);
            AssetManager.ReleaseBuiltinTexture(BuiltinTextureShape.Circle, 40);
            AssetManager.ReleaseBuiltinTexture(BuiltinTextureShape.Diamond, 44);
            AssetManager.ReleaseBuiltinTexture(BuiltinTextureShape.Hexagon, 38);
            AssetManager.ReleaseBuiltinTexture(BuiltinTextureShape.Saw, 56);
            AssetManager.ReleaseBuiltinTexture(BuiltinTextureShape.Star, 24);
            AssetManager.ReleaseBuiltinTexture(BuiltinTextureShape.Pixel, 1);
            _groundPixel = null;
            _triangleTex = null;
            _squareTex = null;
            _circleTex = null;
            _diamondTex = null;
            _hexTex = null;
            _sawTex = null;
            _pickupTex = null;
        }
    }
}
